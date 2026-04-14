using System.Data;
using src.Data;
using src.Models;
using src.Dtos;

using Microsoft.EntityFrameworkCore;
using IntervencoesAPI.Services;
using src.Dtos.Cliente_Tag_Dtos;

namespace src.Services;

/// <summary>
/// Application service for querying and managing <see cref="Cliente_Tag"/> records.
/// </summary>
public class Cliente_Tag_Services
{

    /// <summary>
    /// EF Core database context used to access <see cref="Cliente_Tag"/> entities.
    /// </summary>
    private readonly StocksContext _context;

    /// <summary>
    /// Logger used to record failures and operational errors.
    /// </summary>
    private readonly ILogger<Cliente_Tag_Services> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="Cliente_Tag_Services"/>.
    /// </summary>
    /// <param name="context">EF Core database context used to access <see cref="Cliente_Tag"/> entities.</param>
    /// <param name="logger">Logger used to record failures and operational errors.</param>
    public Cliente_Tag_Services(StocksContext context, ILogger<Cliente_Tag_Services> logger)
    {
        _context = context;
        _logger = logger;
    }



    /// <summary>
    /// Retrieves all <see cref="Cliente_Tag"/> records from the database, ordered by Id.
    /// </summary>
    /// <returns>A list of all <see cref="Cliente_Tag"/> entities.</returns>
    /// <exception cref="Exception">Rethrows any exception encountered during database access.</exception>
    public List<Cliente_Tag> GetAll()
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().OrderBy(i => i.Id).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetAll));
            throw;
        }
    }


    /// <summary>
    /// Gets a paginated list of <see cref="Cliente_Tag"/> entities.
    /// </summary>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <returns>A paged list containing the requested page of <see cref="Cliente_Tag"/> entities.</returns>
    /// <exception cref="Exception">Rethrows any exception after logging.</exception>
    public async Task<PagedList<Cliente_Tag>> GetAllPaged(PageParameters pageParameters)
    {
        try
        {
            var query = _context.Cliente_Tags.AsNoTracking().OrderBy(i => i.Id).AsQueryable();
            _logger.LogInformation("CRUD {CrudOperation} {Resource} pageNumber={PageNumber} pageSize={PageSize}",
                "Read",
                "Cliente_Tag",
                pageParameters.PageNumber,
                pageParameters.PageSize);
            return await PagedList<Cliente_Tag>.CreateAsync(query, pageParameters.PageNumber, pageParameters.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database  error in {Method}", nameof(GetAllPaged));
            throw;
        }
    }



    /// <summary>
    /// Retrieves a <see cref="Cliente_Tag"/> entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tag.</param>
    /// <returns>The <see cref="Cliente_Tag"/> entity if found; otherwise, null.</returns>
    /// <exception cref="Exception">Rethrows any exception encountered during database access.</exception>
    public Cliente_Tag? GetById(int id)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(i => i.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetById));
            throw;
        }
    }

    /// <summary>
    /// Gets a tag by unit identifier.
    /// </summary>
    /// <param name="unidade">The unit identifier associated with the tag.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Tag"/> whose Unidade field matches the provided value using no-tracking query.
    /// Returns null if no matching record is found.
    /// </remarks>

    public Cliente_Tag? GetByUnidade(string unidade)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(u => u.Unidade == unidade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByUnidade));
            throw;
        }
    }


    /// <summary>
    /// Gets a tag by location identifier.
    /// </summary>
    /// <param name="Localizacao">The location identifier associated with the tag.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Tag"/> whose Localizacao field matches the provided value using no-tracking query.
    /// Returns null if no matching record is found.
    /// </remarks>
    public Cliente_Tag? GetByLocalizacao(string Localizacao)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(l => l.Localizacao == Localizacao);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByLocalizacao));
            throw;
        }
    }



    /// <summary>
    /// Gets a tag by product identifier.
    /// </summary>
    /// <param name="Produto">The product identifier associated with the tag.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Tag"/> whose Produto field matches the provided value using no-tracking query.
    /// Returns null if no matching record is found.
    /// </remarks>

    public Cliente_Tag? GetByProduto(string Produto)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(p => p.Produto == Produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByProduto));
            throw;
        }
    }


    /// <summary>
    /// Gets a tag by EPC identifier.
    /// </summary>
    /// <param name="EPC">The Electronic Product Code (EPC) uniquely identifying the RFID tag.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Tag"/> whose EPC field matches the provided value using no-tracking query.
    /// Returns null if no matching record is found.
    /// </remarks>

    public Cliente_Tag? GetByEPC(string EPC)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(e => e.EPC == EPC);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByEPC));
            throw;
        }
    }



    /// <summary>
    /// Gets a tag by status identifier.
    /// </summary>
    /// <param name="Estado">The status identifier associated with the tag.</param>
    /// <remarks>
    /// Retrieves a <see cref="Cliente_Tag"/> whose Estado field matches the provided value using no-tracking query.
    /// Returns null if no matching record is found.
    /// </remarks>

    public Cliente_Tag? GetByEstado(string Estado)
    {
        try
        {
            return _context.Cliente_Tags.AsNoTracking().FirstOrDefault(e => e.Estado == Estado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(GetByEstado));
            throw;
        }
    }
    /// <summary>
    /// Creates a new <see cref="Cliente_Tag"/> entity in the database.
    /// </summary>
    /// <param name="dto">The data transfer object containing tag creation data.</param>
    /// <returns>The created <see cref="Cliente_Tag"/> entity.</returns>
    /// <exception cref="Exception">Rethrows any exception encountered during creation.</exception>
    public async Task<Cliente_Tag> CreateTag(CreateTag dto)
    {
        try
        {
            var tag = new Cliente_Tag
            {
                Unidade = dto.Unidade,
                Localizacao = dto.Localizacao,
                Produto = dto.Produto,
                EPC = dto.EPC,
                Estado = dto.Estado,
                Data_Ultimo_Movimento = DateTime.UtcNow,
                Ultima_Localizacao = dto.Ultima_Localizacao,
                Ultima_Unidade = dto.Ultima_Unidade
            };
            _context.Cliente_Tags.Add(tag);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Created a new tag => {tag}");
            return tag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(CreateTag));
            throw;
        }
    }

    /// <summary>
    /// Updates an existing <see cref="Cliente_Tag"/> entity with new data.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to update.</param>
    /// <param name="dto">The data transfer object containing updated tag data.</param>
    /// <returns>The updated <see cref="Cliente_Tag"/> entity if found; otherwise, null.</returns>
    /// <exception cref="Exception">Rethrows any exception encountered during update.</exception>
    public async Task<Cliente_Tag?> Update(int id, UpdateTag dto)
    {
        try
        {
            var tag = _context.Cliente_Tags.FirstOrDefault(i => i.Id == id);
            if (tag is null)
            {
                return null;
            }
            tag.Unidade = dto.Unidade;
            tag.Localizacao = dto.Localizacao;
            tag.Produto = dto.Produto;
            tag.EPC = dto.EPC;
            tag.Estado = dto.Estado;
            tag.Ultima_Localizacao = dto.Ultima_Localizacao;
            tag.Ultima_Unidade = dto.Ultima_Unidade;

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Tag Updated with id => {id} and changed => {tag}");
            return tag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(Update));
            throw;
        }
    }

    /// <summary>
    /// Deletes a <see cref="Cliente_Tag"/> entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the tag to delete.</param>
    /// <returns>True if the tag was deleted; otherwise, false.</returns>
    /// <exception cref="Exception">Rethrows any exception encountered during deletion.</exception>
    public bool Delete(int id)
    {
        try
        {
            var tag = _context.Cliente_Tags.FirstOrDefault(i => i.Id == id);
            if (tag is null)
            {
                return false;
            }
            _context.Cliente_Tags.Remove(tag);
            _context.SaveChanges();
            _logger.LogInformation($"tag with id => {id} was deleted!!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Method}", nameof(Delete));
            throw;
        }
    }
}
