using System.Data;
using System.Globalization;
using src.Data;
using src.Models;
using src.Dtos.Movimentos_Dtos;
using src.Services.TimeSeries;
using Expedita.Export.Excel;

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

    private static void ValidatePesquisaBase(string hotel, int tab, DateTime dataInicio, DateTime dataFim, int tipo)
    {
        if (dataInicio > dataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }

        if (string.IsNullOrWhiteSpace(hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }
    }

    private async Task<(TimeSeriesGranularity Granularity, List<PesquisaItem> Items)> LoadPesquisaSeriesAsync(
        string hotel,
        int tab,
        DateTime dataInicio,
        DateTime dataFim,
        int tipo)
    {
        ValidatePesquisaBase(hotel, tab, dataInicio, dataFim, tipo);

        var (startInclusive, endInclusive, granularity) = TimeSeriesTabs.GetWindowWithGranularity(dataInicio, dataFim, tab);
        var buckets = TimeSeriesTabs.GetBuckets(startInclusive, endInclusive, granularity).ToList();

        // Base query uses the movements table filtered by the computed window.
        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

        async Task<List<PesquisaItem>> BuildSerieAsync(int direcao)
        {
            IQueryable<Cliente_Movimento> movimentos = direcao == 0
                ? baseQuery.Where(m => m.Para == hotel)
                : baseQuery.Where(m => m.Para == LavandariaPara).Where(m => m.De == hotel);

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

        if (tipo is 0 or 2)
        {
            result.AddRange(await BuildSerieAsync(0));
        }

        if (tipo is 1 or 2)
        {
            result.AddRange(await BuildSerieAsync(1));
        }

        return (granularity, result
            .OrderBy(r => r.Data)
            .ThenBy(r => r.Direcao)
            .ToList());
    }
    

    /// <summary>
    /// Searches movimentos within a date range and returns entries/exits (or both) for a given hotel.
    /// </summary>
    /// <param name="request">Search criteria including date range, hotel, optional document number, and direction type.</param>
    /// <returns>A list of <see cref="PesquisaItem"/> ordered by movement date.</returns>
    /// <exception cref="ArgumentException">Thrown when request parameters are invalid.</exception>
    public async Task<List<PesquisaItem>> PesquisaAsync(PesquisaRequest request)
    {
        ValidatePesquisaBase(request.Hotel, request.Tab, request.DataInicio, request.DataFim, request.Tipo);

        if (request.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (request.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        var (_, result) = await LoadPesquisaSeriesAsync(
            request.Hotel,
            request.Tab,
            request.DataInicio,
            request.DataFim,
            request.Tipo);

        // Pesquisa series is zero-filled for charting; API consumers typically want only real rows.
        result = result
            .Where(x => x.IdDoc > 0)
            .ToList();

        var skip = (request.Pagina - 1) * request.NumRegistos;

        return result
            .Skip(skip)
            .Take(request.NumRegistos)
            .ToList();
    }

    /// <summary>
    /// Generates an Excel (.xlsx) export for the Pesquisa series.
    /// </summary>
    /// <remarks>
    /// This export returns the <b>full</b> series for the requested window (no pagination).
    /// </remarks>
    public async Task<(byte[] Content, string FileName)> PesquisaExcelAsync(PesquisaExcelRequest request)
    {
        var (granularity, items) = await LoadPesquisaSeriesAsync(
            request.Hotel,
            request.Tab,
            request.DataInicio,
            request.DataFim,
            request.Tipo);

        // Keep Excel export consistent with the JSON endpoint: export only real rows.
        items = items
            .Where(x => x.IdDoc > 0)
            .ToList();

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

        // Allocate one spare row/column to avoid any edge-indexing behavior in the library.
        var numRows = Math.Max(2, items.Count + 2); // header + data + spare
        const int numCols = 8; // 7 data columns + spare

        var workDir = Path.Combine(Path.GetTempPath(), "StocksAPI", "exports");
        Directory.CreateDirectory(workDir);

        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_pesquisa.xlsx";
        var fullPath = Path.Combine(workDir, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        Stream? fileStream = null;

        try
        {
            var doc = new xlsxDocumento(workDir);
            var page = doc.AdicionarPagina("Pesquisa", numRows, numCols);

            // Header row
            SetCell(page, col0: 0, row0: 0, "Data", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 1, row0: 0, "Hora", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 2, row0: 0, "Direcao", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 3, row0: 0, "Tipo", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 4, row0: 0, "Produto", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 5, row0: 0, "Qtd", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 6, row0: 0, "IdDoc", xlsxCelula.tiposValor.TextoHeader);

            // Data rows
            for (var i = 0; i < items.Count; i++)
            {
                var row0 = i + 1;
                var item = items[i];

                SetCell(page, col0: 0, row0, FormatData(item.Data, granularity), xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 1, row0, FormatHora(item.Data, granularity), xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 2, row0, item.Direcao.ToString(CultureInfo.InvariantCulture), xlsxCelula.tiposValor.Inteiro);
                SetCell(page, col0: 3, row0, item.Tipo, xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 4, row0, item.Produto ?? string.Empty, xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 5, row0, item.Qtd.ToString(CultureInfo.InvariantCulture), xlsxCelula.tiposValor.Inteiro);
                SetCell(page, col0: 6, row0, item.IdDoc.ToString(CultureInfo.InvariantCulture), xlsxCelula.tiposValor.Inteiro);
            }

            doc.Exportar(fileName, xlsxDocumento.tipoExportacao.documentoXls, workDir, ref fileStream);
            fileStream?.Flush();
        }
        finally
        {
            fileStream?.Dispose();
        }

        var content = await File.ReadAllBytesAsync(fullPath);

        try
        {
            File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temporary Excel export file: {Path}", fullPath);
        }

        return (content, fileName);
    }

    /// <summary>
    /// Writes a single cell into an <see cref="xlsxPagina"/>.
    /// </summary>
    /// <remarks>
    /// In C#, the compiler generates explicit accessors like <c>set_Celula(row, col, cell)</c>.
    ///
    /// Important details:
    /// - The accessor parameter order is <b>(row, col)</b>, not (col, row)
    /// - Indices are treated as <b>0-based</b> by the accessor
    ///
    /// If you swap row/col, or use the wrong base, the library can throw "Index was out of range".
    /// </remarks>
    private static void SetCell(xlsxPagina page, int col0, int row0, string? value, xlsxCelula.tiposValor tipo)
    {
        // Normalize nulls so we never pass null strings into the library.
        var safeValue = value ?? string.Empty;

        // The cell object stores its own coordinates, so we set both:
        // 1) the xlsxCelula ctor coords
        // 2) the explicit idxLinha/idxColuna fields
        var cell = new xlsxCelula(col0, row0)
        {
            idxColuna = col0,
            idxLinha = row0,
            valor = safeValue,
            tipoValor = tipo
        };

        // NOTE: this is row/col order.
        page.set_Celula(row0, col0, cell);
    }

    /// <summary>
    /// Validates the common (non-pagination) parameters for Evolução requests.
    /// </summary>
    /// <remarks>
    /// keep this separate so:
    /// - JSON endpoint can validate pagination
    /// - Excel endpoint can ignore pagination entirely
    /// but both share the same domain rules (hotel, tab range, date window, tipo range).
    /// </remarks>
    private static void ValidateEvolucaoBase(string hotel, int tab, DateTime dataInicio, DateTime dataFim, int tipo)
    {
        if (tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (string.IsNullOrWhiteSpace(hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (tab == 4 && dataInicio > dataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }
    }

    /// <summary>
    /// Loads all data needed to build the Evolução series (time buckets + aggregates per direction).
    /// </summary>
    /// <remarks>
    /// This method is the shared "core" for:
    /// - <see cref="EvolucaoAsync"/> (JSON): applies API pagination to the final series.
    ///
    /// What it returns:
    /// - Buckets: the timeline points (hours/days/months) to render. Buckets are pre-generated, so
    ///   we can output zeros for periods with no movimentos.
    /// - Granularity: Hour/Day/Month, derived from <c>tab</c> and the requested window.
    /// - EntradasMap / SaidasMap: aggregated quantity per bucket for the selected direction(s).
    ///
    /// Why use maps:
    /// We aggregate in SQL (fast), then later build the full bucketed series in memory by
    /// looking up each bucket in a dictionary (fast and predictable ordering).
    /// </remarks>
    private async Task<(List<DateTime> Buckets, TimeSeriesGranularity Granularity, Dictionary<DateTime, int> EntradasMap, Dictionary<DateTime, int> SaidasMap)>
        LoadEvolucaoDataAsync(string hotel, int tab, DateTime dataInicio, DateTime dataFim, int tipo)
    {
        ValidateEvolucaoBase(hotel, tab, dataInicio, dataFim, tipo);

        var (startInclusive, endInclusive, granularity) = TimeSeriesTabs.GetWindowWithGranularity(dataInicio, dataFim, tab);
        var buckets = TimeSeriesTabs.GetBuckets(startInclusive, endInclusive, granularity).ToList();

        // Base query: only movimentos inside the computed window.
        //  use AsNoTracking because these are read-only analytics queries.
        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

        Task<Dictionary<DateTime, int>> LoadAggregatesAsync(IQueryable<Cliente_Movimento> movimentos)
        {
            // For each granularity, group by the appropriate key and sum Quantidade.
            // always normalize the bucket DateTime so later dictionary lookups match.
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

        // Direction rules (matches the existing JSON logic):
        // - entradas: Para == hotel
        // - saidas:   Para == "Lavandaria" AND De == hotel
        if (tipo is 0 or 2)
        {
            entradasMap = await LoadAggregatesAsync(baseQuery.Where(m => m.Para == hotel));
        }

        if (tipo is 1 or 2)
        {
            saidasMap = await LoadAggregatesAsync(
                baseQuery.Where(m => m.Para == LavandariaPara)
                         .Where(m => m.De == hotel));
        }

        return (buckets, granularity, entradasMap, saidasMap);
    }

    /// <summary>
    /// Builds the Evolução series as a flat list of <see cref="EvolucaoItem"/>, including zero-filled buckets.
    /// </summary>
    /// <remarks>
    /// The returned list interleaves directions depending on <paramref name="tipo"/>:
    /// - tipo=0: only entradas
    /// - tipo=1: only saidas
    /// - tipo=2: entradas + saidas
    ///
    /// Pagination is implemented using an <c>index</c> that counts output rows (not buckets), since
    /// when tipo=2 we output two rows per time bucket.
    /// </remarks>
    private static List<EvolucaoItem> BuildEvolucaoResult(
        IReadOnlyList<DateTime> buckets,
        TimeSeriesGranularity granularity,
        Dictionary<DateTime, int> entradasMap,
        Dictionary<DateTime, int> saidasMap,
        int tipo,
        int skip,
        int take)
    {
        // Formatting helpers: these match the original JSON endpoint behavior.
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

        var result = new List<EvolucaoItem>(capacity: Math.Min(Math.Max(take, 32), 32_768));
        var index = 0;

        foreach (var bucket in buckets)
        {
            if (tipo is 0 or 2)
            {
                // entradas
                var qtd = entradasMap.TryGetValue(bucket, out var value) ? value : 0;
                if (index >= skip && result.Count < take)
                {
                    result.Add(new EvolucaoItem(
                        Data: FormatData(bucket, granularity),
                        Hora: FormatHora(bucket, granularity),
                        Direcao: 0,
                        Qtd: qtd));
                }

                index++;
                if (result.Count >= take)
                {
                    break;
                }
            }

            if (tipo is 1 or 2)
            {
                // saidas
                var qtd = saidasMap.TryGetValue(bucket, out var value) ? value : 0;
                if (index >= skip && result.Count < take)
                {
                    result.Add(new EvolucaoItem(
                        Data: FormatData(bucket, granularity),
                        Hora: FormatHora(bucket, granularity),
                        Direcao: 1,
                        Qtd: qtd));
                }

                index++;
                if (result.Count >= take)
                {
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Computes an evolution series of quantities for entries/exits (or both), honoring preset tabs (day/week/month/year/custom).
    /// </summary>
    /// <param name="request">Evolution criteria including date range, hotel, optional document number, direction type, and pagination.</param>
    /// <returns>A list of <see cref="EvolucaoItem"/> ordered by date (and hour when applicable).</returns>
    /// <exception cref="ArgumentException">Thrown when request parameters are invalid.</exception>
    public async Task<List<EvolucaoItem>> EvolucaoAsync(EvolucaoRequest request)
    {
        // JSON endpoint requires explicit pagination.
        if (request.Pagina < 1)
        {
            throw new ArgumentException("'pagina' must be >= 1.");
        }

        if (request.NumRegistos < 1)
        {
            throw new ArgumentException("'numRegistos' must be >= 1.");
        }

        var (buckets, granularity, entradasMap, saidasMap) = await LoadEvolucaoDataAsync(
            request.Hotel,
            request.Tab,
            request.DataInicio,
            request.DataFim,
            request.Tipo);

        var skip = (request.Pagina - 1) * request.NumRegistos;

        return BuildEvolucaoResult(
            buckets,
            granularity,
            entradasMap,
            saidasMap,
            request.Tipo,
            skip,
            request.NumRegistos);
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