using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using IntervencoesAPI.Dtos;
using IntervencoesAPI.Dtos.ClienteDtos;
using Pages.Services;
using src.Models;
using src.Services;
using Microsoft.Extensions.Logging;

namespace src.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly ClienteService _clienteService;
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(ClienteService clienteService, ILogger<ClienteController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of clientes.
    /// </summary>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <remarks>
    /// Returns a paged result containing the requested page of clientes.
    /// </remarks>
    /// <response code="200">Clientes returned successfully.</response>
    /// <response code="400">The query parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<GetCliente>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedList<GetCliente>>> GetAll([FromQuery] PageParameters pageParameters)
    {
        {
            _logger.LogInformation(
                "CRUD {CrudOperation} {Resource} pageNumber={PageNumber} pageSize={PageSize}",
                "Read",
                "Cliente",
                pageParameters.PageNumber,
                pageParameters.PageSize);

            var pagedClientes = await _clienteService.GetAllPagedAsync(pageParameters);
            return Ok(pagedClientes);
        }
    }

    /// <summary>
    /// Gets a cliente by identifier.
    /// </summary>
    /// <param name="id">The cliente identifier.</param>
    /// <remarks>
    /// Returns the cliente resource if found.
    /// </remarks>
    /// <response code="200">Cliente returned successfully.</response>
    /// <response code="404">Cliente not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetCliente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCliente>> GetById(int id)
    {
        {
            _logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Read", "Cliente", id);

            var cliente = await _clienteService.GetByIdCliente(id);
            if (cliente is null)
            {
                return NotFound($"No Cliente exists with the provided ID: {id}.");
            }

            return Ok(cliente);
        }
    }

    /// <summary>
    /// Gets a cliente by its reference value.
    /// </summary>
    /// <param name="referencia">The reference value to search for.</param>
    /// <remarks>
    /// Returns the cliente resource that matches the provided <paramref name="referencia"/>.
    /// </remarks>
    /// <response code="200">Cliente returned successfully.</response>
    /// <response code="400">The query parameter is missing or invalid.</response>
    /// <response code="404">Cliente not found.</response>
    [HttpGet("Refencia")]
    [HttpGet("Referencia")]
    [ProducesResponseType(typeof(GetCliente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetCliente>> GetByReferencia([FromQuery] string referencia)
    {
        if (string.IsNullOrWhiteSpace(referencia))
        {
            return BadRequest("Query parameter 'referencia' is required.");
        }

        var cliente = await _clienteService.GetByReferencia(referencia);
        if (cliente is null)
        {
            return NotFound($"No Cliente exists with the provided Referencia: {referencia}.");
        }

        return Ok(cliente);
    }

    /// <summary>
    /// Gets clientes for a given entidade identifier (paged).
    /// </summary>
    /// <param name="idEntidade">The entidade identifier to filter clientes by.</param>
    /// <param name="pageParameters">Pagination parameters (page number and page size).</param>
    /// <remarks>
    /// Returns a paged result containing clientes associated with the provided <paramref name="idEntidade"/>.
    /// </remarks>
    /// <response code="200">Clientes returned successfully.</response>
    /// <response code="400">The query parameters are invalid.</response>
    /// <response code="404">No clientes were found for the provided entidade identifier.</response>
    [HttpGet("IdEntidade")]
    [ProducesResponseType(typeof(PagedList<GetCliente>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedList<GetCliente>>> GetByIdEntidade([FromQuery] int idEntidade, [FromQuery] PageParameters pageParameters)
    {
        if (idEntidade <= 0)
        {
            return BadRequest("Query parameter 'idEntidade' must be a positive integer.");
        }

        _logger.LogInformation(
            "CRUD {CrudOperation} {Resource} idEntidade={IdEntidade} pageNumber={PageNumber} pageSize={PageSize}",
            "Read",
            "Cliente",
            idEntidade,
            pageParameters.PageNumber,
            pageParameters.PageSize);

        var pagedClientes = await _clienteService.GetByIdEntidadePagedAsync(idEntidade, pageParameters);
        if (pagedClientes.TotalCount == 0)
        {
            return NotFound($"No Cliente exists with the provided IdEntidade: {idEntidade}.");
        }

        return Ok(pagedClientes);
    }

    /// <summary>
    /// Creates a new cliente.
    /// </summary>
    /// <param name="dto">The data used to create a new cliente.</param>
    /// <remarks>
    /// Creates a new cliente and returns the created resource with its generated identifier.
    /// </remarks>
    /// <response code="201">Cliente created successfully.</response>
    /// <response code="400">The request data is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Cliente), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Cliente>> Create([FromBody] CreateCliente dto)
    {
        {
            _logger.LogInformation("CRUD {CrudOperation} {Resource}", "Create", "Cliente");

            var created = await _clienteService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);

        }
    }

    /// <summary>
    /// Updates an existing cliente.
    /// </summary>
    /// <param name="id">The cliente identifier.</param>
    /// <param name="updatecliente">The data used to update the cliente.</param>
    /// <remarks>
    /// Updates the cliente identified by <paramref name="id"/> and returns the updated resource.
    /// </remarks>
    /// <response code="200">Cliente updated successfully.</response>
    /// <response code="400">The request data is invalid.</response>
    /// <response code="404">Cliente not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Cliente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Cliente>> Update(int id, [FromBody] UpdateCliente updatecliente)
    {
        _logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Update", "Cliente", id);

        var updatedCliente = await _clienteService.UpdateAsync(id, updatecliente);

        if (updatedCliente is null)
        {
            return NotFound($"No Cliente exists with the provided ID: {id}.");
        }

        return Ok(updatedCliente);
    }


    /// <summary>
    /// Deletes an existing cliente.
    /// </summary>
    /// <param name="id">The cliente identifier.</param>
    /// <remarks>
    /// Deletes the cliente identified by <paramref name="id"/>.
    /// </remarks>
    /// <response code="204">Cliente deleted successfully.</response>
    /// <response code="404">Cliente not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult Delete(int id)
    {
        {
            _logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Delete", "Cliente", id);

            var deleted = _clienteService.Delete(id);
            if (!deleted)
            {
                return NotFound($"No Cliente exists with the provided ID: {id}.");
            }
            return NoContent();
        }
    }



}
