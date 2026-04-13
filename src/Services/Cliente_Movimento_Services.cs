using System.Data;
using src.Data;
using src.Models;
using src.Dtos.Cliente_Movimento_Dtos;
using src.Dtos.Movimentos_Dtos;

using Microsoft.EntityFrameworkCore;
using IntervencoesAPI.Services;


namespace src.Services;

public class Cliente_Movimento_Services
{
    private readonly StocksContext _context;
    private readonly ILogger<Cliente_Movimento_Services> _logger;

    private const string LavandariaPara = "Lavandaria";
    private const string TodasUnidadesToken = "todas-unidades";

    private sealed class ProximaEntregaProjection
    {
        public string UnidadeHotel { get; init; } = string.Empty;
        public DateTime Data { get; init; }
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

    public async Task<List<PesquisaItem>> PesquisaAsync(PesquisaRequest request)
    {
        if (request.DataInicio > request.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }

        if (request.Tipo is < 0 or > 2)
        {
            throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ");
        }

        IQueryable<VwClienteMovimento> baseQuery = _context.VwClienteMovimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= request.DataInicio && m.Datetime <= request.DataFim);

        if (!string.IsNullOrWhiteSpace(request.NumGuia))
        {
            baseQuery = baseQuery.Where(m => m.MovementRID == request.NumGuia);
        }

        var entradas = baseQuery
            .Where(m => m.Para == request.Hotel)
            .Select(m => new
            {
                m.Id,
                m.MovementRID,
                Data = m.Datetime,
                Direcao = 0,
                Tipo = "Renting",
                Produto = m.Descricao,
                Qtd = m.Quantidade
            });

        var saidas = baseQuery
            .Where(m => m.Para == LavandariaPara)
            .Select(m => new
            {
                m.Id,
                m.MovementRID,
                Data = m.Datetime,
                Direcao = 1,
                Tipo = "Renting",
                Produto = m.Descricao,
                Qtd = m.Quantidade
            });

        var query = request.Tipo switch
        {
            0 => entradas,
            1 => saidas,
            2 => entradas.Concat(saidas),
            _ => throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ")
        };

        return await query
            .OrderBy(i => i.Data)
            .Select(i => new PesquisaItem(
                NumDocumento: i.MovementRID,
                Data: i.Data,
                Direcao: i.Direcao,
                Tipo: i.Tipo,
                Produto: i.Produto,
                Qtd: i.Qtd,
                IdDoc: i.Id))
            .ToListAsync();
    }

    public async Task<List<EvolucaoItem>> EvolucaoAsync(EvolucaoRequest request)
    {
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

        var isHourly = request.DataInicio.Date == request.DataFim.Date;

        IQueryable<VwClienteMovimento> baseQuery = _context.VwClienteMovimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= request.DataInicio && m.Datetime <= request.DataFim);

        if (!string.IsNullOrWhiteSpace(request.NumGuia))
        {
            baseQuery = baseQuery.Where(m => m.MovementRID == request.NumGuia);
        }

        var skip = (request.Pagina - 1) * request.NumRegistos;

        if (isHourly)
        {
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
            var entradas = baseQuery
                .Where(m => m.Para == request.Hotel)
                .GroupBy(m => m.Datetime.Date)
                .Select(g => new
                {
                    Dia = g.Key,
                    Direcao = 0,
                    Qtd = g.Sum(x => x.Quantidade)
                });

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

    public async Task<List<ProximaEntregaItem>> ProximasEntregasAsync(ProximasEntregasRequest request, string? hotelQuery)
    {
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

        var hotelFiltro = string.IsNullOrWhiteSpace(hotelQuery) ? request.Hotel : hotelQuery;
        var now = DateTime.Now;

        IQueryable<Cliente_Movimento> baseQuery = _context.Cliente_Movimentos
            .AsNoTracking()
            .Where(m => m.Datetime >= request.DataInicio && m.Datetime <= request.DataFim)
            .Where(m => m.Datetime >= now);

        if (!string.IsNullOrWhiteSpace(request.NumGuia))
        {
            baseQuery = baseQuery.Where(m => m.MovementRID == request.NumGuia);
        }

        var isTodasUnidades = string.Equals(hotelFiltro, TodasUnidadesToken, StringComparison.OrdinalIgnoreCase);

        IQueryable<ProximaEntregaProjection> entradas = baseQuery
            .Where(m => m.Para != null && m.Para != LavandariaPara)
            .Where(m => isTodasUnidades || m.Para == hotelFiltro)
            .GroupBy(m => m.Para!)
            .Select(g => new ProximaEntregaProjection
            {
                UnidadeHotel = g.Key,
                Data = g.Min(x => x.Datetime)
            });

        IQueryable<ProximaEntregaProjection> saidas = baseQuery
            .Where(m => m.Para == LavandariaPara)
            .Where(m => m.De != null)
            .Where(m => isTodasUnidades || m.De == hotelFiltro)
            .GroupBy(m => m.De!)
            .Select(g => new ProximaEntregaProjection
            {
                UnidadeHotel = g.Key,
                Data = g.Min(x => x.Datetime)
            });

        IQueryable<ProximaEntregaProjection> query = request.Tipo switch
        {
            0 => entradas,
            1 => saidas,
            2 => entradas.Concat(saidas)
                .GroupBy(x => x.UnidadeHotel)
                .Select(g => new ProximaEntregaProjection
                {
                    UnidadeHotel = g.Key,
                    Data = g.Min(x => x.Data)
                }),
            _ => throw new ArgumentException("'tipo' must be 0 (entrada), 1 (saida), or 2 (ambos). ")
        };

        var skip = (request.Pagina - 1) * request.NumRegistos;

        return await query
            .OrderBy(x => x.UnidadeHotel)
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
            var clientemovimento = new Cliente_Movimento
            {
                MovementRID = dto.MovementRID,
                De = dto.De,
                Para = dto.Para,
                Cliente = dto.Cliente,
                Descricao = dto.Descricao,
                DataFormatada = dto.DataFormatada,
                Datetime = DateTime.UtcNow,
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
            c.MovementRID = dto.MovementRID;
            c.De = dto.De;
            c.Para = dto.Para;
            c.Cliente = dto.Cliente;
            c.Descricao = dto.Descricao;
            c.DataFormatada = dto.DataFormatada;
            c.Datetime = DateTime.UtcNow;
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
