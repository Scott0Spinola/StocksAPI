using src.Data;
using src.Dtos.Dashboard_Dtos;
using Microsoft.EntityFrameworkCore;

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

    private static (DateTime startInclusive, DateTime endInclusive) GetWindow(EntradasSaidasIndicadorRequest request)
    {
        var anchorDay = request.DataFim.Date;

        return request.Tab switch
        {
            0 => (anchorDay, anchorDay.AddDays(1).AddTicks(-1)),
            1 => (anchorDay.AddDays(-6), anchorDay.AddDays(1).AddTicks(-1)),
            2 => (anchorDay.AddDays(-30), anchorDay.AddDays(1).AddTicks(-1)),
            3 => (new DateTime(anchorDay.Year, anchorDay.Month, 1).AddMonths(-11), anchorDay.AddDays(1).AddTicks(-1)),
            4 => (request.DataInicio, request.DataFim),
            _ => (request.DataInicio, request.DataFim)
        };
    }

    private static (DateTime startInclusive, DateTime endInclusive) GetPreviousWindow(DateTime startInclusive, DateTime endInclusive)
    {
        var duration = endInclusive - startInclusive;
        var previousEndInclusive = startInclusive.AddTicks(-1);
        var previousStartInclusive = previousEndInclusive - duration;
        return (previousStartInclusive, previousEndInclusive);
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

        var (startInclusive, endInclusive) = GetWindow(request);
        var (previousStartInclusive, previousEndInclusive) = GetPreviousWindow(startInclusive, endInclusive);

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
            "IndicadorEntradasSaidas hotel={Hotel} tab={Tab} current={Start}-{End} previous={PrevStart}-{PrevEnd} entradas={Entradas} saidas={Saidas}",
            request.Hotel,
            request.Tab,
            startInclusive,
            endInclusive,
            previousStartInclusive,
            previousEndInclusive,
            entradasCurrent,
            saidasCurrent);

        // Peso ainda nao esta disponibilizado no feed de movimentos.
        const decimal pesoKg = 0m;

        var diferencaCurrent = saidasCurrent - entradasCurrent;
        var diferencaPrevious = saidasPrevious - entradasPrevious;

        return new EntradasSaidasIndicadorResponse(
            Saidas: new IndicadorDirecao(NumPecas: saidasCurrent, NumPecasAnterior: saidasPrevious, Peso: pesoKg),
            Entradas: new IndicadorDirecao(NumPecas: entradasCurrent, NumPecasAnterior: entradasPrevious, Peso: pesoKg),
            Diferenca: new IndicadorDirecao(NumPecas: diferencaCurrent, NumPecasAnterior: diferencaPrevious, Peso: pesoKg));
    }
}