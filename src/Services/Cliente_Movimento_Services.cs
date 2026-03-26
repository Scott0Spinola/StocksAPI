using System.Data;
using src.Data;
using src.Models;
using src.Dtos.Cliente_Movimento_Dtos;

using Microsoft.EntityFrameworkCore;
using IntervencoesAPI.Services;


namespace src.Services;

public class Cliente_Movimento_Services
{
    private readonly StocksContext _context;
    private readonly ILogger<Cliente_Movimento_Services> _logger;


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
            return _context.Cliente_Movimentos.AsNoTracking().FirstOrDefault(i => i.Id == id);
        }
        catch (System.Exception)
        {

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
        catch (System.Exception)
        {

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
        catch (System.Exception)
        {

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
        catch (System.Exception)
        {

            throw;
        }
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
        catch (System.Exception)
        {

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
        catch (System.Exception)
        {

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
