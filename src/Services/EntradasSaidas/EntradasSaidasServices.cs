using System.Data;
using src.Data;
using src.Models;
using src.Dtos.Movimentos_Dtos;
using src.Services.TimeSeries;

using Microsoft.EntityFrameworkCore;


namespace src.Services.EntradasSaidasService;

public class EntradasSaidasService
{   
    private readonly StocksContext _context;
    private readonly ILogger<EntradasSaidasService> _logger;
    
    /// <summary>
    /// Initializes a new instance of <see cref="EntradasSaidasService"/>.
    /// </summary>
    /// <param name="context">EF Core database context used to access <see cref="Cliente_Movimento"/> entities.</param>
    /// <param name="logger">Logger used to record failures and operational errors.</param>
    public EntradasSaidasService(StocksContext context, ILogger<EntradasSaidasService> logger)
    {
        _context = context;
        _logger = logger;
    }
    private const string LavandariaPara = "Lavandaria";

    private sealed class ProximaEntregaProjection
    {
        public string UnidadeHotel { get; init; } = string.Empty;
        public DateTime Data { get; init; }
    }



    private static string? ToProduto(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            return null;
        }

        return descricao.Length <= 100 ? descricao : descricao[..100];
    }
    

    /// <summary>
    /// Searches movimentos within a date range and returns entries/exits (or both) for a given hotel.
    /// </summary>
    /// <param name="request">Search criteria including date range, hotel, optional document number, and direction type.</param>
    /// <returns>A list of <see cref="PesquisaItem"/> ordered by movement date.</returns>
    /// <exception cref="ArgumentException">Thrown when request parameters are invalid.</exception>
    public async Task<List<PesquisaItem>> PesquisaAsync(PesquisaRequest request)
    {
        // Validate date interval and direction selector early for predictable API behavior.
        if (request.DataInicio > request.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (request.Tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (request.Tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }

        if (string.IsNullOrWhiteSpace(request.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (request.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (request.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        var (startInclusive, endInclusive, granularity) = TimeSeriesTabs.GetWindowWithGranularity(request.DataInicio, request.DataFim, request.Tab);
        var buckets = TimeSeriesTabs.GetBuckets(startInclusive, endInclusive, granularity).ToList();

        // Base query uses the movements table filtered by the computed window.
        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

        

        async Task<List<PesquisaItem>> BuildSerieAsync(int direcao)
        {
            IQueryable<Cliente_Movimento> movimentos = direcao == 0
                ? baseQuery.Where(m => m.Para == request.Hotel)
                : baseQuery.Where(m => m.Para == LavandariaPara).Where(m => m.De == request.Hotel);

            Dictionary<DateTime, (int Qtd, int MinId)> map;
            Dictionary<int, string?> descricaoById;

            if (granularity == TimeSeriesGranularity.Hour)
            {
                var aggregates = await movimentos
                    .GroupBy(m => new { m.Datetime.Year, m.Datetime.Month, m.Datetime.Day, m.Datetime.Hour })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        g.Key.Hour,
                        Qtd = g.Sum(x => x.Quantidade),
                        MinId = g.Min(x => x.Id)
                    })
                    .ToListAsync();

                var ids = aggregates.Select(x => x.MinId).Distinct().ToList();
                descricaoById = await movimentos
                    .Where(m => ids.Contains(m.Id))
                    .Select(m => new { m.Id, m.Descricao })
                    .ToDictionaryAsync(x => x.Id, x => x.Descricao);

                map = aggregates.ToDictionary(
                    x => new DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0),
                    x => (x.Qtd, x.MinId));
            }
            else if (granularity == TimeSeriesGranularity.Day)
            {
                var aggregates = await movimentos
                    .GroupBy(m => new { m.Datetime.Year, m.Datetime.Month, m.Datetime.Day })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        g.Key.Day,
                        Qtd = g.Sum(x => x.Quantidade),
                        MinId = g.Min(x => x.Id)
                    })
                    .ToListAsync();

                var ids = aggregates.Select(x => x.MinId).Distinct().ToList();
                descricaoById = await movimentos
                    .Where(m => ids.Contains(m.Id))
                    .Select(m => new { m.Id, m.Descricao })
                    .ToDictionaryAsync(x => x.Id, x => x.Descricao);

                map = aggregates.ToDictionary(
                    x => new DateTime(x.Year, x.Month, x.Day),
                    x => (x.Qtd, x.MinId));
            }
            else
            {
                var aggregates = await movimentos
                    .GroupBy(m => new { m.Datetime.Year, m.Datetime.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Qtd = g.Sum(x => x.Quantidade),
                        MinId = g.Min(x => x.Id)
                    })
                    .ToListAsync();

                var ids = aggregates.Select(x => x.MinId).Distinct().ToList();
                descricaoById = await movimentos
                    .Where(m => ids.Contains(m.Id))
                    .Select(m => new { m.Id, m.Descricao })
                    .ToDictionaryAsync(x => x.Id, x => x.Descricao);

                map = aggregates.ToDictionary(
                    x => new DateTime(x.Year, x.Month, 1),
                    x => (x.Qtd, x.MinId));
            }

            return buckets
                .Select(bucket =>
                {
                    if (map.TryGetValue(bucket, out var value))
                    {
                        descricaoById.TryGetValue(value.MinId, out var descricao);
                        return new PesquisaItem(
                            NumDocumento: null,
                            Data: bucket,
                            Direcao: direcao,
                            Tipo: "Renting",
                            Produto: ToProduto(descricao),
                            Qtd: value.Qtd,
                            IdDoc: value.MinId);
                    }

                    return new PesquisaItem(
                        NumDocumento: null,
                        Data: bucket,
                        Direcao: direcao,
                        Tipo: "Renting",
                        Produto: null,
                        Qtd: 0,
                        IdDoc: 0);
                })
                .ToList();
        }

        var result = new List<PesquisaItem>();

        if (request.Tipo is 0 or 2)
        {
            result.AddRange(await BuildSerieAsync(0));
        }

        if (request.Tipo is 1 or 2)
        {
            result.AddRange(await BuildSerieAsync(1));
        }

        var skip = (request.Pagina - 1) * request.NumRegistos;

        return result
            .OrderBy(r => r.Data)
            .ThenBy(r => r.Direcao)
            .Skip(skip)
            .Take(request.NumRegistos)
            .ToList();
    }

    /// <summary>
    /// Computes an evolution series of quantities for entries/exits (or both), honoring preset tabs (day/week/month/year/custom).
    /// </summary>
    /// <param name="request">Evolution criteria including date range, hotel, optional document number, direction type, and pagination.</param>
    /// <returns>A list of <see cref="EvolucaoItem"/> ordered by date (and hour when applicable).</returns>
    /// <exception cref="ArgumentException">Thrown when request parameters are invalid.</exception>
    public async Task<List<EvolucaoItem>> EvolucaoAsync(EvolucaoRequest request)
    {
        // Validate required invariants up front.
        if (request.Tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (string.IsNullOrWhiteSpace(request.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (request.Tab == 4 && request.DataInicio > request.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (request.Tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }

        if (request.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (request.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        var (startInclusive, endInclusive, granularity) = TimeSeriesTabs.GetWindowWithGranularity(request.DataInicio, request.DataFim, request.Tab);
        var buckets = TimeSeriesTabs.GetBuckets(startInclusive, endInclusive, granularity).ToList();

        // Base query from the movements table filtered by the computed window, with an optional RID filter.
        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

      
        Task<Dictionary<DateTime, int>> LoadAggregatesAsync(IQueryable<Cliente_Movimento> movimentos)
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

            // Month
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

        // API pagination is applied after building the (zero-filled) series.
        var skip = (request.Pagina - 1) * request.NumRegistos;
        var result = new List<EvolucaoItem>(capacity: Math.Min(request.NumRegistos, 512));

        var index = 0;
        foreach (var bucket in buckets)
        {
            if (request.Tipo is 0 or 2)
            {
                var qtd = entradasMap.TryGetValue(bucket, out var value) ? value : 0;
                if (index >= skip && result.Count < request.NumRegistos)
                {
                    result.Add(new EvolucaoItem(
                        Data: FormatData(bucket, granularity),
                        Hora: FormatHora(bucket, granularity),
                        Direcao: 0,
                        Qtd: qtd));
                }

                index++;
                if (result.Count >= request.NumRegistos)
                {
                    break;
                }
            }

            if (request.Tipo is 1 or 2)
            {
                var qtd = saidasMap.TryGetValue(bucket, out var value) ? value : 0;
                if (index >= skip && result.Count < request.NumRegistos)
                {
                    result.Add(new EvolucaoItem(
                        Data: FormatData(bucket, granularity),
                        Hora: FormatHora(bucket, granularity),
                        Direcao: 1,
                        Qtd: qtd));
                }

                index++;
                if (result.Count >= request.NumRegistos)
                {
                    break;
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Lists upcoming deliveries within the given date range.
    /// </summary>
    /// <param name="request">Criteria including date range, direction type, paging, and optional movement document number.</param>
    /// <returns>A paged list of <see cref="ProximaEntregaItem"/> ordered by scheduled date/time.</returns>
    /// <exception cref="ArgumentException">Thrown when request parameters are invalid.</exception>
    public async Task<List<ProximaEntregaItem>> ProximasEntregasAsync(ProximasEntregasRequest request)
    {
        // Validate request parameters.
        if (request.DataInicio > request.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (request.Tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }

        if (request.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (request.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        if (string.IsNullOrWhiteSpace(request.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        var now = DateTime.Now;

        // Only future movements (>= now) are relevant for "next delivery".
        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= request.DataInicio && m.Datetime <= request.DataFim)
            .Where(m => m.Datetime >= now)
            .Where(m => m.Cliente == request.Hotel);

        // Return rows per unidade + scheduled datetime, selecting MIN(Datetime) per group.
        // This shows all deliveries within the window while still deduping same-time duplicates.
        IQueryable<ProximaEntregaProjection> query = request.Tipo switch
        {
            0 => baseQuery
                .Where(m => m.Para != null && m.Para != LavandariaPara)
                .GroupBy(m => new { UnidadeHotel = m.Para!, m.Datetime })
                .Select(g => new ProximaEntregaProjection
                {
                    UnidadeHotel = g.Key.UnidadeHotel,
                    Data = g.Min(x => x.Datetime)
                }),

            1 => baseQuery
                .Where(m => m.Para == LavandariaPara && m.De != null)
                .GroupBy(m => new { UnidadeHotel = m.De!, m.Datetime })
                .Select(g => new ProximaEntregaProjection
                {
                    UnidadeHotel = g.Key.UnidadeHotel,
                    Data = g.Min(x => x.Datetime)
                }),

            2 => baseQuery
                .Where(m => (m.Para != null && m.Para != LavandariaPara)
                            || (m.Para == LavandariaPara && m.De != null))
                .Select(m => new ProximaEntregaProjection
                {
                    UnidadeHotel = m.Para != null && m.Para != LavandariaPara ? m.Para! : m.De!,
                    Data = m.Datetime
                })
                .GroupBy(x => new { x.UnidadeHotel, x.Data })
                .Select(g => new ProximaEntregaProjection
                {
                    UnidadeHotel = g.Key.UnidadeHotel,
                    Data = g.Min(x => x.Data)
                }),

            _ => throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ")
        };

        var skip = (request.Pagina - 1) * request.NumRegistos;

        return await query
            .OrderBy(x => x.Data)
            .ThenBy(x => x.UnidadeHotel)
            .Skip(skip)
            .Take(request.NumRegistos)
            .Select(x => new ProximaEntregaItem(
                DataPrevista: x.Data.ToString("dd/MM/yyyy"),
                HoraPrevista: x.Data.ToString("HH:mm"),
                UnidadeHotel: x.UnidadeHotel,
                Observacoes: string.Empty))
            .ToListAsync();
    }
}