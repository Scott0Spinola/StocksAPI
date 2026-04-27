using src.Data;
using src.Dtos.Dashboard_Dtos;
using Microsoft.EntityFrameworkCore;
using src.Services.TimeSeries;

namespace src.Services.Dashboard;

public class DashboardService
{
    private readonly StocksContext _context;
    private readonly ILogger<DashboardService> _logger;

    private const string LavandariaPara = "Lavandaria";

    public DashboardService(StocksContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EntradasSaidasIndicadorResponse> IndicadorEntradasSaidasAsync(EntradasSaidasIndicadorRequest request)
    {
        if (request.DataInicio > request.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (request.Tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (string.IsNullOrWhiteSpace(request.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        var (startInclusive, endInclusive) = TimeSeriesTabs.GetWindow(request.DataInicio, request.DataFim, request.Tab);
        var (previousStartInclusive, previousEndInclusive) = TimeSeriesTabs.GetPreviousWindow(startInclusive, endInclusive);

        var referenceDate = request.DataFim.Date;
        var cutoffDate = referenceDate.AddDays(-30);

        var numPecasSemMovimento30dias = await _context.Cliente_Tags
            .AsNoTracking()
            .Where(t => t.Unidade == request.Hotel && t.Data_Ultimo_Movimento <= cutoffDate)
            .CountAsync();

        var hotelKey = request.Hotel.Trim().ToUpperInvariant();

        int alerta = 0;
        try
        {
            alerta = await _context.VwIntervencoesAlertasPorHotel
                .AsNoTracking()
                .Where(x => x.Hotel == hotelKey)
                .Select(x => (int?)x.Alerta)
                .FirstOrDefaultAsync() ?? 0;
        }
        catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException sqlEx && sqlEx.Number == 208)
        {
            // View not created in the target database yet.
            _logger.LogWarning(
                "Missing view dbo.vw_IntervencoesAlertasPorHotel; returning 0 alertas. Ensure the view exists in database {Database}.",
                _context.Database.GetDbConnection().Database);
        }

        IQueryable<Models.Cliente_Movimento> current = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

        IQueryable<Models.Cliente_Movimento> previous = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= previousStartInclusive && m.Datetime <= previousEndInclusive);

        // EF Core DbContext instances are not thread-safe; run queries sequentially.
        var entradasCurrent = await current
            .Where(m => m.Para == request.Hotel)
            .SumAsync(m => (int?)m.Quantidade) ?? 0;

        var entradasPrevious = await previous
            .Where(m => m.Para == request.Hotel)
            .SumAsync(m => (int?)m.Quantidade) ?? 0;

        var saidasCurrent = await current 
            .Where(m => m.Para == LavandariaPara && m.De == request.Hotel)
            .SumAsync(m => (int?)m.Quantidade) ?? 0;

        var saidasPrevious = await previous
            .Where(m => m.Para == LavandariaPara && m.De == request.Hotel)
            .SumAsync(m => (int?)m.Quantidade) ?? 0;

        _logger.LogInformation(
            "IndicadorEntradasSaidas hotel={Hotel} tab={Tab} current={Start}-{End} previous={PrevStart}-{PrevEnd} entradas={Entradas} saidas={Saidas} semMovimento30d={SemMovimento30d} refDate={ReferenceDate}",
            request.Hotel,
            request.Tab,
            startInclusive,
            endInclusive,
            previousStartInclusive,
            previousEndInclusive,
            entradasCurrent,
            saidasCurrent,
            numPecasSemMovimento30dias,
            referenceDate);

        // Peso ainda nao esta disponibilizado no feed de movimentos.
        const decimal pesoKg = 0m;

        var diferencaCurrent = saidasCurrent - entradasCurrent;
        var diferencaPrevious = saidasPrevious - entradasPrevious;

        return new EntradasSaidasIndicadorResponse(
            Saidas: new IndicadorDirecao(NumPecas: saidasCurrent, NumPecasAnterior: saidasPrevious, Peso: pesoKg),
            Entradas: new IndicadorDirecao(NumPecas: entradasCurrent, NumPecasAnterior: entradasPrevious, Peso: pesoKg),
            Diferenca: new IndicadorDirecao(NumPecas: diferencaCurrent, NumPecasAnterior: diferencaPrevious, Peso: pesoKg),
            NumPecasSemMovimento30dias: numPecasSemMovimento30dias,
            Alerta: alerta);
    }

    public async Task<List<UltimasDescargasItem>> UltimasDescargasAsync(UltimasDescargasRequest request)
    {
        if (request.Tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (string.IsNullOrWhiteSpace(request.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (request.Tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }

        if (request.Tab == 4 && request.DataInicio > request.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        var (startInclusive, endInclusive, granularity) = TimeSeriesTabs.GetWindowWithGranularity(request.DataInicio, request.DataFim, request.Tab);
        var buckets = TimeSeriesTabs.GetBuckets(startInclusive, endInclusive, granularity).ToList();

        IQueryable<Models.Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

        Task<Dictionary<DateTime, int>> LoadAggregatesAsync(IQueryable<Models.Cliente_Movimento> movimentos)
        {
            if (granularity == TimeSeriesGranularity.Hour)
            {
                return movimentos
                    .GroupBy(m => new { m.Datetime.Year, m.Datetime.Month, m.Datetime.Day, m.Datetime.Hour })
                    .Select(g => new
                    {
                        Bucket = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, 0, 0),
                        Qtd = g.Sum(x => x.Quantidade)
                    })
                    .ToDictionaryAsync(x => x.Bucket, x => x.Qtd);
            }

            if (granularity == TimeSeriesGranularity.Day)
            {
                return movimentos
                    .GroupBy(m => m.Datetime.Date)
                    .Select(g => new
                    {
                        Bucket = g.Key,
                        Qtd = g.Sum(x => x.Quantidade)
                    })
                    .ToDictionaryAsync(x => x.Bucket, x => x.Qtd);
            }

            return movimentos
                .GroupBy(m => new { m.Datetime.Year, m.Datetime.Month })
                .Select(g => new
                {
                    Bucket = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Qtd = g.Sum(x => x.Quantidade)
                })
                .ToDictionaryAsync(x => x.Bucket, x => x.Qtd);
        }

        Dictionary<DateTime, int> entradasMap = new();
        Dictionary<DateTime, int> saidasMap = new();

        if (request.Tipo is 0 or 2)
        {
            entradasMap = await LoadAggregatesAsync(baseQuery.Where(m => m.Para == request.Hotel));
        }

        if (request.Tipo is 1 or 2)
        {
            saidasMap = await LoadAggregatesAsync(
                baseQuery.Where(m => m.Para == LavandariaPara)
                         .Where(m => m.De == request.Hotel));
        }

        static string FormatData(DateTime bucket, TimeSeriesGranularity granularity)
        {
            return granularity == TimeSeriesGranularity.Month
                ? bucket.ToString("MM/yyyy")
                : bucket.ToString("dd/MM");
        }

        static string FormatHora(DateTime bucket, TimeSeriesGranularity granularity)
        {
            return granularity == TimeSeriesGranularity.Hour
                ? bucket.Hour.ToString("00")
                : string.Empty;
        }

        var ordered = new List<UltimasDescargasItem>(capacity: buckets.Count * (request.Tipo == 2 ? 2 : 1));

        foreach (var bucket in buckets)
        {
            if (request.Tipo is 0 or 2)
            {
                var qtd = entradasMap.TryGetValue(bucket, out var value) ? value : 0;
                ordered.Add(new UltimasDescargasItem(
                    Data: FormatData(bucket, granularity),
                    Hora: FormatHora(bucket, granularity),
                    Direcao: 0,
                    Qtd: qtd));
            }

            if (request.Tipo is 1 or 2)
            {
                var qtd = saidasMap.TryGetValue(bucket, out var value) ? value : 0;
                ordered.Add(new UltimasDescargasItem(
                    Data: FormatData(bucket, granularity),
                    Hora: FormatHora(bucket, granularity),
                    Direcao: 1,
                    Qtd: qtd));
            }
        }

        _logger.LogInformation(
            "UltimasDescargas hotel={Hotel} tab={Tab} tipo={Tipo} window={Start}-{End} rows={Rows}",
            request.Hotel,
            request.Tab,
            request.Tipo,
            startInclusive,
            endInclusive,
            ordered.Count);

        return ordered;
    }
}