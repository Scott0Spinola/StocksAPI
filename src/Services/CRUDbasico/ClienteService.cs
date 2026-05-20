using System.Data;
using IntervencoesAPI.Dtos;
using IntervencoesAPI.Dtos.ClienteDtos;
using Microsoft.EntityFrameworkCore;
using Pages.Services;
using src.Data;
using src.Models;

namespace src.Services;


public class ClienteService
{
    private readonly StocksContext _context;

    private readonly ILogger<ClienteService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ClienteService"/>.
    /// </summary>
    /// <param name="context">EF Core database context used to access <see cref="Cliente"/> entities.</param>
    /// <param name="logger">Logger used to record failures and operational errors.</param>
    public ClienteService(StocksContext context, ILogger<ClienteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all <see cref="Cliente"/> records ordered by identifier.
    /// </summary>
    /// <remarks>
    /// This query is tracked by EF Core (no <c>AsNoTracking</c>). Use paged or no-tracking variants
    /// where appropriate.
    /// </remarks>
    /// <returns>All clientes ordered by <see cref="Cliente.Id"/>.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public List<Cliente> GetAll()
    {
        try
        {
            return _context.Clientes
                .AsNoTracking()
                .AsSplitQuery()
                .Include(c => c.Entidade)
                .OrderBy(i => i.Id)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetAll));
            throw;
        }
    }


    /// <summary>
    /// Gets a paginated list of clientes.
    /// </summary>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <returns>A paged list containing the requested page of clientes as DTOs.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<PagedList<GetCliente>> GetAllPagedAsync(PageParameters pageParameters)
    {
        try
        {
            var query = _context.Clientes
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new GetCliente(
                    c.Id,
                    c.IdEntidade,
                    c.Nome,
                    c.Referencia,
                    c.Observacoes,
                    c.Estado,
                    c.NProcesso,
                    c.DataDeInicio,
                    c.DataActualizacao,
                    c.CliCampo1,
                    c.CliCampo2,
                    c.CliCampo3,
                    c.CliCampo4
                ));

            return await PagedList<GetCliente>.CreateAsync(query, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetAllPagedAsync));
            throw;
        }
    }

    /// <summary>
    /// Gets a single <see cref="Cliente"/> by its identifier.
    /// </summary>
    /// <param name="id">The cliente identifier.</param>
    /// <returns>The matching cliente, or <see langword="null"/> if not found.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<GetCliente?>  GetByIdCliente(int id)
    {
        try
        {
            return await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new GetCliente(
                    c.Id,
                    c.IdEntidade,
                    c.Nome,
                    c.Referencia,
                    c.Observacoes,
                    c.Estado,
                    c.NProcesso,
                    c.DataDeInicio,
                    c.DataActualizacao,
                    c.CliCampo1,
                    c.CliCampo2,
                    c.CliCampo3,
                    c.CliCampo4
                ))
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByIdCliente));
            throw;
        }
    }


    /// <summary>
    /// Gets a single <see cref="Cliente"/> by its <see cref="Cliente.Referencia"/>.
    /// </summary>
    /// <param name="referencia">The reference value to look up.</param>
    /// <returns>The matching cliente, or <see langword="null"/> if not found.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<GetCliente?> GetByReferencia(string referencia)
    {
        try
        {
            return  await _context.Clientes
                .AsNoTracking()
                .Where(c => c.Referencia == referencia)
                .Select(c => new GetCliente(
                    c.Id,
                    c.IdEntidade,
                    c.Nome,
                    c.Referencia,
                    c.Observacoes,
                    c.Estado,
                    c.NProcesso,
                    c.DataDeInicio,
                    c.DataActualizacao,
                    c.CliCampo1,
                    c.CliCampo2,
                    c.CliCampo3,
                    c.CliCampo4
                ))
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByReferencia));
            throw;
        }
    }

    /// <summary>
    /// Gets the first cliente that matches the given entidade identifier.
    /// </summary>
    /// <param name="idEntidade">The entidade identifier.</param>
    /// <returns>The first matching cliente, or <see langword="null"/> if none exists.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<GetCliente?> GetByIdEntidade(int idEntidade)
    {
        try
        {
            return await _context.Clientes
                .AsNoTracking()
                .Where(c => c.IdEntidade == idEntidade)
                .Select(c => new GetCliente(
                    c.Id,
                    c.IdEntidade,
                    c.Nome,
                    c.Referencia,
                    c.Observacoes,
                    c.Estado,
                    c.NProcesso,
                    c.DataDeInicio,
                    c.DataActualizacao,
                    c.CliCampo1,
                    c.CliCampo2,
                    c.CliCampo3,
                    c.CliCampo4
                ))
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByIdEntidade));
            throw;
        }
    }

    /// <summary>
    /// Gets clientes that match the given entidade identifier, returned as a paged result.
    /// </summary>
    /// <param name="idEntidade">The entidade identifier to filter clientes by.</param>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <returns>A paged list containing clientes associated with the provided entidade identifier.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<PagedList<GetCliente>> GetByIdEntidadePagedAsync(int idEntidade, PageParameters pageParameters)
    {
        try
        {
            var query =  _context.Clientes
                .AsNoTracking()
                .Where(c => c.IdEntidade == idEntidade)
                .OrderBy(c => c.Id)
                .Select(c => new GetCliente(
                    c.Id,
                    c.IdEntidade,
                    c.Nome,
                    c.Referencia,
                    c.Observacoes,
                    c.Estado,
                    c.NProcesso,
                    c.DataDeInicio,
                    c.DataActualizacao,
                    c.CliCampo1,
                    c.CliCampo2,
                    c.CliCampo3,
                    c.CliCampo4
                ))
                .AsQueryable();

            return await PagedList<GetCliente>.CreateAsync(query, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByIdEntidadePagedAsync));
            throw;
        }
    }

    /// <summary>
    /// Creates a new <see cref="Cliente"/> from the provided DTO.
    /// </summary>
    /// <remarks>
    /// Sets <see cref="Cliente.DataDeInicio"/> and <see cref="Cliente.DataActualizacao"/> to
    /// <see cref="DateTime.UtcNow"/>.
    /// </remarks>
    /// <param name="dto">The values to use for creation.</param>
    /// <returns>The created cliente after it has been persisted.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<Cliente> CreateAsync(CreateCliente dto)
    {
        try
        {
            var cliente = new Cliente
            {
                IdEntidade = dto.IdEntidade,
                Referencia = dto.Referencia,
                Observacoes = dto.Observacoes,
                Estado = dto.Estado,
                NProcesso = dto.NProcesso,
                Nome = dto.Nome,
                DataDeInicio = DateTime.UtcNow,
                DataActualizacao = DateTime.UtcNow,
                CliCampo1 = dto.CliCampo1,
                CliCampo2 = dto.CliCampo2,
                CliCampo3 = dto.CliCampo3,
                CliCampo4 = dto.CliCampo4,
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }
       catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Operation Canceled - {Method}", nameof(CreateAsync));
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error in {Method}", nameof(CreateAsync));
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database update error in {Method}", nameof(CreateAsync));
            throw;
        }
    }

    /// <summary>
    /// Updates an existing <see cref="Cliente"/> identified by <paramref name="id"/>.
    /// </summary>
    /// <remarks>
    /// Updates <see cref="Cliente.DataActualizacao"/> to <see cref="DateTime.UtcNow"/>.
    /// </remarks>
    /// <param name="id">The cliente identifier.</param>
    /// <param name="dto">The values to apply to the existing cliente.</param>
    /// <returns>The updated cliente, or <see langword="null"/> if no cliente exists with the given identifier.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<Cliente?> UpdateAsync(int id, UpdateCliente dto)
    {
        try
        {
            var cliente = _context.Clientes.FirstOrDefault(i => i.Id == id);

            if (cliente is null)
            {
                return null;
            }
            cliente.IdEntidade = dto.IdEntidade;
            cliente.Referencia = dto.Referencia;
            cliente.Observacoes = dto.Observacoes;
            cliente.Estado = dto.Estado;
            cliente.NProcesso = dto.NProcesso;
            cliente.Nome = dto.Nome;
            cliente.DataActualizacao = DateTime.UtcNow;
            cliente.CliCampo1 = dto.CliCampo1;
            cliente.CliCampo2 = dto.CliCampo2;
            cliente.CliCampo3 = dto.CliCampo3;
            cliente.CliCampo4 = dto.CliCampo4;

            await _context.SaveChangesAsync();
            return cliente;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency error in {Method}", nameof(UpdateAsync));
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Database update error in {Method}", nameof(UpdateAsync));
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method} id={Id}", nameof(UpdateAsync), id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a <see cref="Cliente"/> identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The cliente identifier.</param>
    /// <returns><see langword="true"/> if the cliente existed and was deleted; otherwise <see langword="false"/>.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public bool Delete(int id)
    {
        try
        {
            var cliente = _context.Clientes.FirstOrDefault(i => i.Id == id);
            if (cliente is null)
            {
                return false;
            }
            _context.Clientes.Remove(cliente);
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