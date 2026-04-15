using System.Data;
using src.Data;
using src.Models;
using src.Dtos.Cliente_Movimento_Dtos;
using System.Globalization;
using src.Dtos.Movimentos_Dtos;

using Microsoft.EntityFrameworkCore;
using IntervencoesAPI.Services;


namespace src.Services;

/// <summary>
/// Application service for querying and managing <see cref="Cliente_Movimento"/> records.
/// </summary>
public class Cliente_Movimento_Services
{
    private readonly StocksContext _context;
    private readonly ILogger<Cliente_Movimento_Services> _logger;

    private const string LavandariaPara = "Lavandaria";

    private sealed class ProximaEntregaProjection
    {
        public string UnidadeHotel { get; init; } = string.Empty;
        public DateTime Data { get; init; }
    }

    private enum PesquisaGranularity
    {
        Hour,
        Day,
        Month
    }

    private static IEnumerable<DateTime> GetBuckets(DateTime startInclusive, DateTime endInclusive, PesquisaGranularity granularity)
    {
        if (granularity == PesquisaGranularity.Hour)
        {
            var start = new DateTime(startInclusive.Year, startInclusive.Month, startInclusive.Day, 0, 0, 0);
            for (var hour = 0; hour < 24; hour++)
            {
                yield return start.AddHours(hour);
            }

            yield break;
        }

        if (granularity == PesquisaGranularity.Day)
        {
            for (var day = startInclusive.Date; day <= endInclusive.Date; day = day.AddDays(1))
            {
                yield return day;
            }

            yield break;
        }

        // Month
        var startMonth = new DateTime(startInclusive.Year, startInclusive.Month, 1);
        var endMonth = new DateTime(endInclusive.Year, endInclusive.Month, 1);
        for (var month = startMonth; month <= endMonth; month = month.AddMonths(1))
        {
            yield return month;
        }
    }

    private static (DateTime startInclusive, DateTime endInclusive, PesquisaGranularity granularity) GetPesquisaWindow(PesquisaRequest request)
    {
        // For preset tabs, use DataFim as the anchor "current" day for deterministic behavior.
        var anchorDay = request.DataFim.Date;

        return request.Tab switch
        {
            0 => (anchorDay, anchorDay.AddDays(1).AddTicks(-1), PesquisaGranularity.Hour),
            1 => (anchorDay.AddDays(-6), anchorDay.AddDays(1).AddTicks(-1), PesquisaGranularity.Day),
            2 => (anchorDay.AddDays(-30), anchorDay.AddDays(1).AddTicks(-1), PesquisaGranularity.Day),
            3 => (new DateTime(anchorDay.Year, anchorDay.Month, 1).AddMonths(-11), anchorDay.AddDays(1).AddTicks(-1), PesquisaGranularity.Month),
            4 => (request.DataInicio, request.DataFim, request.DataInicio.Date == request.DataFim.Date ? PesquisaGranularity.Hour : PesquisaGranularity.Day),
            _ => (request.DataInicio, request.DataFim, PesquisaGranularity.Day)
        };
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Cliente_Movimento_Services"/>.
    /// </summary>
    /// <param name="context">EF Core database context used to access <see cref="Cliente_Movimento"/> entities.</param>
    /// <param name="logger">Logger used to record failures and operational errors.</param>
    public Cliente_Movimento_Services(StocksContext context, ILogger<Cliente_Movimento_Services> logger)
    {
        _context = context;
        _logger = logger;
    }


    /// <summary>
    /// Gets all <see cref="Cliente_Movimento"/> records ordered by identifier.
    /// </summary>
    /// <remarks>
    /// This query is tracked by EF Core (no <c>AsNoTracking</c>). Use paged or no-tracking variants
    /// where appropriate.
    /// </remarks>
    /// <returns>All movimentos ordered by <see cref="Cliente_Movimento.Id"/>.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public List<Cliente_Movimento> GetAll()
    {
        try
        {
            return _context.Cliente_Movimentos.AsNoTracking().OrderBy(i => i.Id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetAll));
            throw;
        }
    }

    /// <summary>
    /// Gets a paginated list of Movimentos.
    /// </summary>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <returns>A paged list containing the requested page of clientes movimentos.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<PagedList<Cliente_Movimento>> GetAllPaged(PageParameters pageParameters)
    {
        try
        {
            var query = _context.Cliente_Movimentos.AsNoTracking().OrderBy(i => i.Id).AsQueryable();
            return await PagedList<Cliente_Movimento>.CreateAsync(query, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database  error in {Method}", nameof(GetAllPaged));
            throw;
        }
    }

    /// <summary>
    /// Gets a movimento by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the movimento.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Movimento"/> from the data source using its identifier.
    /// Logs the lookup operation and returns null if no matching record is found.
    /// </remarks>
    public Cliente_Movimento? GetById(int id)
    {
        try
        {
            _logger.LogInformation($"Finding id => {id}.");
            return  _context.Cliente_Movimentos.AsNoTracking().FirstOrDefault(i => i.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetById));
            throw;
        }
    }



    /// <summary>
    /// Gets a movimento by movement RID.
    /// </summary>
    /// <param name="RID">The unique movement RID associated with the movimento.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Movimento"/> whose MovementRID matches the provided value.
    /// Logs the lookup operation and returns null if no matching record is found.
    /// </remarks>
    public Cliente_Movimento? GetByRID(string RID)
    {
        try
        {
            _logger.LogInformation($"Finding MovementRID => {RID}.");
            return _context.Cliente_Movimentos.FirstOrDefault(r => r.MovementRID == RID);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByRID));
            throw;
        }
    }



    /// <summary>
    /// Gets a movimento by client identifier.
    /// </summary>
    /// <param name="cliente">The client identifier associated with the movimento.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Movimento"/> whose Cliente field matches the provided value.
    /// Logs the lookup operation and returns null if no matching record is found.
    /// </remarks>
    public Cliente_Movimento? GetByCliente(string cliente)
    {
        try
        {
            _logger.LogInformation($"Finding cliente => {cliente}.");
            return _context.Cliente_Movimentos.FirstOrDefault(c => c.Cliente == cliente);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByCliente));
            throw;
        }
    }

    /// <summary>
    /// Gets a paged list of movimentos within a specified date interval.
    /// </summary>
    /// <param name="start">The start date of the interval (inclusive).</param>
    /// <param name="end">The end date of the interval (inclusive).</param>
    /// <param name="pageParameters">The pagination parameters to apply to the result set.</param>
    /// <remarks>
    /// Builds a query over <see cref="Cliente_Movimento"/> filtered by the provided date range and ordered by identifier,
    /// then materializes it as a paged list according to the supplied pagination settings.
    /// </remarks>
    public async Task<PagedList<Cliente_Movimento>> GetByIntervaloDate(DateTime start, DateTime end, PageParameters pageParameters)
    {
        try
        {
            var query = _context.Cliente_Movimentos
                .AsNoTracking()
                .Where(d => d.Datetime >= start && d.Datetime <= end)
                .OrderBy(i => i.Id)
                .AsQueryable();
            return await PagedList<Cliente_Movimento>.CreateAsync(query, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByIntervaloDate));
            throw;
        }
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

        var (startInclusive, endInclusive, granularity) = GetPesquisaWindow(request);
        var buckets = GetBuckets(startInclusive, endInclusive, granularity).ToList();

        // Base query uses the movements table filtered by the computed window.
        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= startInclusive && m.Datetime <= endInclusive);

        if (!string.IsNullOrWhiteSpace(request.NumGuia))
        {
            // Optional filter for a specific movement document/RID.
            baseQuery = baseQuery.Where(m => m.MovementRID == request.NumGuia);
        }

        async Task<List<PesquisaItem>> BuildSerieAsync(int direcao)
        {
            IQueryable<Cliente_Movimento> movimentos = direcao == 0
                ? baseQuery.Where(m => m.Para == request.Hotel)
                : baseQuery.Where(m => m.Para == LavandariaPara).Where(m => m.De == request.Hotel);

            Dictionary<DateTime, (int Qtd, int MinId)> map;

            if (granularity == PesquisaGranularity.Hour)
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

                map = aggregates.ToDictionary(
                    x => new DateTime(x.Year, x.Month, x.Day, x.Hour, 0, 0),
                    x => (x.Qtd, x.MinId));
            }
            else if (granularity == PesquisaGranularity.Day)
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

                map = aggregates.ToDictionary(
                    x => new DateTime(x.Year, x.Month, 1),
                    x => (x.Qtd, x.MinId));
            }

            return buckets
                .Select(bucket =>
                {
                    if (map.TryGetValue(bucket, out var value))
                    {
                        return new PesquisaItem(
                            NumDocumento: null,
                            Data: bucket,
                            Direcao: direcao,
                            Tipo: "Renting",
                            Produto: null,
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

        return result
            .OrderBy(r => r.Data)
            .ThenBy(r => r.Direcao)
            .ToList();
    }

    /// <summary>
    /// Computes an evolution series of quantities (hourly for a single day, daily otherwise) for entries/exits (or both).
    /// </summary>
    /// <param name="request">Evolution criteria including date range, hotel, optional document number, direction type, and pagination.</param>
    /// <returns>A list of <see cref="EvolucaoItem"/> ordered by date (and hour when applicable).</returns>
    /// <exception cref="ArgumentException">Thrown when request parameters are invalid.</exception>
    public async Task<List<EvolucaoItem>> EvolucaoAsync(EvolucaoRequest request)
    {
        // Validate required invariants up front.
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

        // If the interval is a single calendar day, the API returns an hourly series.
        var isHourly = request.DataInicio.Date == request.DataFim.Date;

        // Base query from the view filtered by date range, with an optional RID filter.
        IQueryable<VwClienteMovimento> baseQuery = _context.VwClienteMovimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= request.DataInicio && m.Datetime <= request.DataFim);

        if (!string.IsNullOrWhiteSpace(request.NumGuia))
        {
            baseQuery = baseQuery.Where(m => m.MovementRID == request.NumGuia);
        }

        // API pagination is applied after the grouping.
        var skip = (request.Pagina - 1) * request.NumRegistos;

        if (isHourly)
        {
            // Hourly entries: group by date+hour and sum quantities.
            var entradas = baseQuery
                .Where(m => m.Para == request.Hotel)
                .GroupBy(m => new { Dia = m.Datetime.Date, Hora = m.Datetime.Hour })
                .Select(g => new
                {
                    g.Key.Dia,
                    g.Key.Hora,
                    Direcao = 0,
                    Qtd = g.Sum(x => x.Quantidade)
                });

            // Hourly exits: group by date+hour and sum quantities.
            var saidas = baseQuery
                .Where(m => m.Para == LavandariaPara)
                .GroupBy(m => new { Dia = m.Datetime.Date, Hora = m.Datetime.Hour })
                .Select(g => new
                {
                    g.Key.Dia,
                    g.Key.Hora,
                    Direcao = 1,
                    Qtd = g.Sum(x => x.Quantidade)
                });

            var query = request.Tipo switch
            {
                0 => entradas,
                1 => saidas,
                2 => entradas.Concat(saidas),
                _ => throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ")
            };

            return await query
                .OrderBy(i => i.Dia)
                .ThenBy(i => i.Hora)
                .Skip(skip)
                .Take(request.NumRegistos)
                .Select(i => new EvolucaoItem(
                    Data: i.Dia.ToString("dd/MM"),
                    Hora: i.Hora.ToString("00"),
                    Direcao: i.Direcao,
                    Qtd: i.Qtd))
                .ToListAsync();
        }
        else
        {
            // Daily entries: group by date and sum quantities.
            var entradas = baseQuery
                .Where(m => m.Para == request.Hotel)
                .GroupBy(m => m.Datetime.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Direcao = 0,
                    Qtd = g.Sum(x => x.Quantidade)
                });

            // Daily exits: group by date and sum quantities.
            var saidas = baseQuery
                .Where(m => m.Para == LavandariaPara)
                .GroupBy(m => m.Datetime.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Direcao = 1,
                    Qtd = g.Sum(x => x.Quantidade)
                });

            var query = request.Tipo switch
            {
                0 => entradas,
                1 => saidas,
                2 => entradas.Concat(saidas),
                _ => throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ")
            };

            return await query
                .OrderBy(i => i.Dia)
                .Skip(skip)
                .Take(request.NumRegistos)
                .Select(i => new EvolucaoItem(
                    Data: i.Dia.ToString("dd/MM"),
                    Hora: string.Empty,
                    Direcao: i.Direcao,
                    Qtd: i.Qtd))
                .ToListAsync();
        }
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
            .Where(m => m.Datetime >= now);

        // Scope results to the requested client/hotel.
        baseQuery = baseQuery.Where(m => m.Cliente == request.Hotel);

        if (!string.IsNullOrWhiteSpace(request.NumGuia))
        {
            baseQuery = baseQuery.Where(m => m.MovementRID == request.NumGuia);
        }

        // Entries: future movements going to hotel units (Para).
        IQueryable<ProximaEntregaProjection> entradas = baseQuery
            .Where(m => m.Para != null && m.Para != LavandariaPara)
            .Select(m => new ProximaEntregaProjection
            {
                UnidadeHotel = m.Para!,
                Data = m.Datetime
            });

        // Exits: future movements leaving hotel units (De) towards the laundry.
        IQueryable<ProximaEntregaProjection> saidas = baseQuery
            .Where(m => m.Para == LavandariaPara)
            .Where(m => m.De != null)
            .Select(m => new ProximaEntregaProjection
            {
                UnidadeHotel = m.De!,
                Data = m.Datetime
            });

        IQueryable<ProximaEntregaProjection> query = request.Tipo switch
        {
            0 => entradas,
            1 => saidas,
            2 => entradas.Concat(saidas),
            _ => throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ")
        };

        // Paging is applied after aggregation.
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
    
    /// <summary>
    /// Creates a new movimento from the provided data transfer object.
    /// </summary>
    /// <param name="dto">The data used to construct the new movimento.</param>
    /// <remarks>
    /// Maps the incoming <c>Create</c> DTO to a new <see cref="Cliente_Movimento"/>, sets the current UTC timestamp,
    /// persists it to the data store, and returns the created entity.
    /// </remarks>
    public async Task<Cliente_Movimento> CreateMovimento(Create dto)
    {
        try
        {
            var now = DateTime.UtcNow;
            var clientemovimento = new Cliente_Movimento
            {
                MovementRID = dto.MovementRID,
                De = dto.De,
                Para = dto.Para,
                Cliente = dto.Cliente,
                Descricao = dto.Descricao,
                Datetime = now,
                DataFormatada = now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                Quantidade = dto.Quantidade
            };

            _context.Cliente_Movimentos.Add(clientemovimento);
            await _context.SaveChangesAsync();
            return clientemovimento;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(CreateMovimento));
            throw;
        }
    }



    /// <summary>
    /// Updates an existing movimento by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the movimento to update.</param>
    /// <param name="dto">The updated data for the movimento.</param>
    /// <remarks>
    /// Locates the <see cref="Cliente_Movimento"/> by identifier, applies the updates from the <c>Update</c> DTO,
    /// refreshes the timestamp to current UTC time, persists the changes, and returns the updated entity or null if not found.
    /// </remarks>
    public async Task<Cliente_Movimento?> Update(int id, Update dto)
    {
        try
        {
            var c = _context.Cliente_Movimentos.FirstOrDefault(i => i.Id == id);
            if (c is null)
            {
                return null;
            }

            var now = DateTime.UtcNow;
            c.MovementRID = dto.MovementRID;
            c.De = dto.De;
            c.Para = dto.Para;
            c.Cliente = dto.Cliente;
            c.Descricao = dto.Descricao;
            c.Datetime = now;
            c.DataFormatada = now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            c.Quantidade = dto.Quantidade;

            await _context.SaveChangesAsync();
            return c;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(Update));
            throw;
        }

    }

    /// <summary>
    /// Deletes a movimento by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the movimento to delete.</param>
    /// <remarks>
    /// Locates the <see cref="Cliente_Movimento"/> by identifier, removes it from the data store if found,
    /// and returns true on successful deletion or false if the record does not exist.
    /// </remarks>
    public bool Delete(int id)
    {
        try
        {
            var cliente = _context.Cliente_Movimentos.FirstOrDefault(i => i.Id == id);
            if (cliente is null)
            {
                return false;
            }
            _context.Cliente_Movimentos.Remove(cliente);
            _context.SaveChanges();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(Delete));
            throw;
        }
    }
}
