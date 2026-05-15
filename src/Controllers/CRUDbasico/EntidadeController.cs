using IntervencoesAPI.Dtos;
using IntervencoesAPI.Dtos.EntidadeDtos;
using Pages.Services;
using src.Models;
using src.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace src.Controllers;

/// <summary>
/// Provides endpoints to manage entidades.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntidadeController : ControllerBase
{
	private readonly EntidadeService _entidadeService;
	private readonly ILogger<EntidadeController> _logger;

	public EntidadeController(EntidadeService entidadeService, ILogger<EntidadeController> logger)
	{
		_entidadeService = entidadeService;
		_logger = logger;
	}

	/// <summary>
	/// Gets a paginated list of entidades.
	/// </summary>
	/// <param name="pageParameters">Pagination parameters (page number and page size).</param>
	/// <remarks>
	/// Returns a paged result containing the requested page of entidades.
	/// </remarks>
	/// <response code="200">Entidades returned successfully.</response>
	/// <response code="400">The query parameters are invalid.</response>
	[HttpGet]
	[ProducesResponseType(typeof(PagedList<Entidade>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<PagedList<Entidade>>> GetAll([FromQuery] PageParameters pageParameters)
	{
		{
			_logger.LogInformation(
				"CRUD {CrudOperation} {Resource} pageNumber={PageNumber} pageSize={PageSize}",
				"Read",
				"Entidade",
				pageParameters.PageNumber,
				pageParameters.PageSize);

			var pagedEntidades = await _entidadeService.GetAllPagedAsync(pageParameters);
			return Ok(pagedEntidades);
		}
	}

	/// <summary>
	/// Gets an entidade by identifier.
	/// </summary>
	/// <param name="id">The entidade identifier.</param>
	/// <remarks>
	/// Returns the entidade resource if found.
	/// </remarks>
	/// <response code="200">Entidade returned successfully.</response>
	/// <response code="404">Entidade not found.</response>
	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(Entidade), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public ActionResult<Entidade> GetById(int id)
	{
		{
			_logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Read", "Entidade", id);

			var entidade = _entidadeService.GetByIdEntidade(id);
			if (entidade is null)
			{
				return NotFound($"No Entidade exists with the provided ID: {id}.");
			}

			return entidade;
		}
	}


	/// <summary>
	/// Gets an entidade by nome social.
	/// </summary>
	/// <param name="name">The nome social to search for.</param>
	/// <response code="200">Entidade returned successfully.</response>
	/// <response code="400">The name query parameter is missing/invalid.</response>
	/// <response code="404">Entidade not found.</response>
	[HttpGet("nomeSocial")]
	[ProducesResponseType(typeof(Entidade), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public ActionResult<Entidade> GetNomeSocial([FromQuery] string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return BadRequest("Query parameter 'name' is required.");
		}

		var entidade = _entidadeService.GetByNomeSocial(name);
		if (entidade is null)
		{
			return NotFound($"No Entidade exists with the provided Name: {name}.");
		}

		return Ok(entidade);
	}

	/// <summary>
	/// Gets an entidade by referencia.
	/// </summary>
	/// <param name="referencia">The referencia value to search for.</param>
	/// <remarks>
	/// The value is matched against the <c>Referencia</c> field.
	/// </remarks>
	/// <response code="200">Entidade returned successfully.</response>
	/// <response code="400">The referencia query parameter is missing/invalid.</response>
	/// <response code="404">Entidade not found.</response>
	[HttpGet("Referencia")]
	[ProducesResponseType(typeof(Entidade), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public ActionResult<Entidade> GetByReferencia([FromQuery] string referencia)
	{
		if (string.IsNullOrWhiteSpace(referencia))
		{
			return BadRequest("Query parameter 'referencia' is required.");
		}

		var entidade = _entidadeService.GetByReferencia(referencia);
		if (entidade is null)
		{
			return NotFound($"No Entidade exists with the provided Referencia: {referencia}.");
		}

		return Ok(entidade);
	}




	/// <summary>
	/// Creates a new entidade.
	/// </summary>
	/// <param name="dto">The data used to create a new entidade.</param>
	/// <remarks>
	/// Creates a new entidade and returns the created resource with its generated identifier.
	/// </remarks>
	/// <response code="201">Entidade created successfully.</response>
	/// <response code="400">The request data is invalid.</response>
	[HttpPost]
	[ProducesResponseType(typeof(Entidade), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Entidade>> Create([FromBody] CreateEntidade dto)
	{
		{
			_logger.LogInformation("CRUD {CrudOperation} {Resource}", "Create", "Entidade");

			var created = await _entidadeService.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}
	}

	/// <summary>
	/// Updates an existing entidade.
	/// </summary>
	/// <param name="id">The entidade identifier.</param>
	/// <param name="updateEntidade">The data used to update the entidade.</param>
	/// <remarks>
	/// Updates the entidade identified by <paramref name="id"/> and returns the updated resource.
	/// </remarks>
	/// <response code="200">Entidade updated successfully.</response>
	/// <response code="400">The request data is invalid.</response>
	/// <response code="404">Entidade not found.</response>
	[HttpPut("{id:int}")]
	[ProducesResponseType(typeof(Entidade), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<Entidade>> Update(int id, [FromBody] UpdateEntidade updateEntidade)
	{
		_logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Update", "Entidade", id);

		var updatedEntidade = await _entidadeService.UpdateAsync(id, updateEntidade);

		if (updatedEntidade is null)
		{
			return NotFound($"No Entidade exists with the provided ID: {id}.");
		}

		return Ok(updatedEntidade);
	}

	/// <summary>
	/// Deletes an existing entidade.
	/// </summary>
	/// <param name="id">The entidade identifier.</param>
	/// <remarks>
	/// Deletes the entidade identified by <paramref name="id"/>.
	/// </remarks>
	/// <response code="204">Entidade deleted successfully.</response>
	/// <response code="404">Entidade not found.</response>
	[HttpDelete("{id:int}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public ActionResult Delete(int id)
	{
		{
			_logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Delete", "Entidade", id);

			var deleted = _entidadeService.Delete(id);
			if (!deleted)
			{
				return NotFound($"No Entidade exists with the provided ID: {id}.");
			}

			return NoContent();
		}
	}


	/// <summary>
	/// Gets an entidade with related data (details view).
	/// </summary>
	/// <param name="id">The entidade identifier.</param>
	/// <remarks>
	/// Returns an aggregated DTO that includes the entidade and its related clientes, processos, and intervenções.
	/// </remarks>
	/// <response code="200">Entidade details returned successfully.</response>
	/// <response code="404">Entidade not found.</response>
	[HttpGet("{id:int}/details")]
	[ProducesResponseType(typeof(EntidadeDetailsDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<EntidadeDetailsDto>> GetDetails(int id)
	{
		var dto = await _entidadeService.GetDetailsAsync(id);
		return dto is null ? NotFound() : Ok(dto);
	}




}
