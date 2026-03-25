using System.Data;
using src.Data;
using src.Models;
using src.Dtos;

using Microsoft.EntityFrameworkCore;
using IntervencoesAPI.Services;

namespace src.Services;

public class Cliente_Tag_Services
{
    private readonly StocksContext _context;

    private readonly ILogger<Cliente_Tag_Services> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="Cliente_Tag_Services"/>.
    /// </summary>
    /// <param name="context">EF Core database context used to access <see cref="Cliente_Movimento"/> entities.</param>
    /// <param name="logger">Logger used to record failures and operational errors.</param>
    public Cliente_Tag_Services(StocksContext context, ILogger<Cliente_Tag_Services> logger)
    {
        _context = context;
        _logger = logger;
    }


    public List<Cliente_Tag> GetAll()
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().OrderBy(i => i.Id).ToList();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }


    /// <summary>
    /// Gets a paginated list of Tags.
    /// </summary>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <returns>A paged list containing the requested page of clientes movimentos.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<PagedList<Cliente_Tag>> GetAllPaged(PageParameters pageParameters)
    {
        try
        {
            var query = _context.Cliente_Tags.AsNoTracking().OrderBy(i => i.Id).AsQueryable();
            return await PagedList<Cliente_Tag>.CreateAsync(query, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database  error in {Method}", nameof(GetAllPaged));
            throw;
        }
    }


    
    public Cliente_Tag? GetById(int id)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(i => i.Id == id);
        }
        catch (System.Exception)
        {

            throw;
        }
    }

    
}
