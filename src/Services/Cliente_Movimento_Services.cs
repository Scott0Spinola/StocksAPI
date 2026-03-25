using System.Data;
using src.Data;
using src.Models;
//using src.Dtos;

using Microsoft.EntityFrameworkCore;
using IntervencoesAPI.Services;
using System.Data.Common;
using src.Dtos.Cliente_Movimento_Dtos;

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
    /// Gets a paginated list of clientes.
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


    public Cliente_Movimento? GetById(int id)
    {
        try
        {
            return _context.Cliente_Movimentos.FirstOrDefault(i => i.Id == id);
        }
        catch (System.Exception)
        {

            throw;
        }
    }

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
