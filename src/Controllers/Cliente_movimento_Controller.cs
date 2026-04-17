
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pages.Services;
using src.Dtos.Cliente_Movimento_Dtos;
using src.Models;
using src.Services;

namespace src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Cliente_movimento_Controller : ControllerBase
    {
        private readonly Cliente_Movimento_Services _cliente_Movimento_Service;
        private readonly ILogger<Cliente_movimento_Controller> _logger;

        public Cliente_movimento_Controller(Cliente_Movimento_Services cliente_movimento_services, ILogger<Cliente_movimento_Controller> logger)
        {
            _cliente_Movimento_Service = cliente_movimento_services;
            _logger = logger;
        }

        /// <summary>
        /// Gets a paged list of all movimentos.
        /// </summary>
        /// <param name="pageParameters">The pagination and sorting parameters.</param>
        /// <remarks>
        /// Retrieves a paged collection of <see cref="Cliente_Movimento"/> resources.
        /// Logs the read operation including the requested page number and page size.
        /// </remarks>
        /// <response code="200">Paged movimentos returned successfully.</response>
        /// <response code="404">No movimentos found.</response>

        [HttpGet]
        [ProducesResponseType(typeof(PagedList<Cliente_Movimento>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<PagedList<Cliente_Movimento>>> GetAll([FromQuery] PageParameters pageParameters)
        {
            _logger.LogInformation(
                "CRUD {CrudOperation} {Resource} pageNumber={PageNumber} pageSize={PageSize}",
                "Read",
                "Cliente_Movimento",
                pageParameters.PageNumber,
                pageParameters.PageSize);
            var paged = await _cliente_Movimento_Service.GetAllPaged(pageParameters);
            return Ok(paged);

        }



        /// <summary>
        /// Gets a paged list of movimentos within a specified date interval.
        /// </summary>
        /// <param name="pageParameters">The pagination and sorting parameters.</param>
        /// <param name="start">The start date of the intervalo (inclusive).</param>
        /// <param name="end">The end date of the intervalo (inclusive).</param>
        /// <remarks>
        /// Returns a paged collection of <see cref="Cliente_Movimento"/> filtered by the provided date range.
        /// The start date must be less than or equal to the end date, otherwise a bad request is returned.
        /// </remarks>
        /// <response code="200">Paged movimentos returned successfully.</response>
        /// <response code="400">The provided date range is invalid.</response>
        /// <response code="404">No movimentos found for the specified date range.</response>
        [HttpGet("Por-Data")]
        [ProducesResponseType(typeof(PagedList<Cliente_Movimento>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedList<Cliente_Movimento>>> GetintervaloDatePaged([FromQuery] PageParameters pageParameters, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
            {
                return BadRequest("'start' must be less than or equal to 'end'.");
            }
            var paged = await _cliente_Movimento_Service.GetByIntervaloDate(start, end, pageParameters);
            return Ok(paged);
        }



        /// <summary>
        /// Gets a movimento by identifier.
        /// </summary>
        /// <param name="id">The movimento identifier.</param>
        /// <remarks>
        /// Returns the movimento resource if found.
        /// </remarks>
        /// <response code="200">Movimento returned successfully.</response>
        /// <response code="404">Movimento not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Cliente_Movimento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<Cliente_Movimento> GetById(int id)
        {
            _logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Read", "Cliente_Movimento", id);

            var c = _cliente_Movimento_Service.GetById(id);
            if (c is null)
            {
                return NotFound($"No Movimento exists with the provided ID: {id}.");
            }
            return c;

        }


        [HttpGet("RID")]
        [ProducesResponseType(typeof(Cliente_Movimento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<Cliente_Movimento> GetByRID(string rid)
        {
            
            var c = _cliente_Movimento_Service.GetByRID(rid);
            if (c is null)
            {
                return NotFound($"No Movimento exists with the provided RID: {rid}.");
            }
            return c;

        }



        
        [HttpGet("Cliente")]
        [ProducesResponseType(typeof(Cliente_Movimento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<Cliente_Movimento> GetByCliente(string cliente)
        {
            
            var c = _cliente_Movimento_Service.GetByCliente(cliente);
            if (c is null)
            {
                return NotFound($"No Movimento exists with the provided Cliente: {cliente}.");
            }
            return c;

        }

        
        
        /// <summary>
        /// Creates a new movimento.
        /// </summary>
        /// <param name="dto">The data used to create a new movimento.</param>
        /// <remarks>
        /// Creates a new movimento and returns the created resource with its generated identifier.
        /// </remarks>
        /// <response code="201">Movimento created successfully.</response>
        /// <response code="400">The request data is invalid.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Cliente_Movimento), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<Cliente_Movimento>> CreateMovimento([FromBody] Create dto)
        {
            _logger.LogInformation("CRUD {CrudOperation} {Resource}", "Create", "Movimento");

            var created = await _cliente_Movimento_Service.CreateMovimento(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }



        /// <summary>
        /// Updates an existing movimento.
        /// </summary>
        /// <param name="id">The movimento identifier.</param>
        /// <param name="dto">The data used to update the movimento.</param>
        /// <remarks>
        /// Updates the movimento identified by <paramref name="id"/> and returns the updated resource.
        /// </remarks>
        /// <response code="200">movimento updated successfully.</response>
        /// <response code="400">The request data is invalid.</response>
        /// <response code="404">movimento not found.</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Cliente_Movimento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<Cliente_Movimento>> Update(int id, [FromBody] Update dto)
        {
            _logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Update", "Cliente_Movimento", id);

            var u = await _cliente_Movimento_Service.Update(id, dto);

            if (u is null)
            {
                return NotFound($"No Movimento exists with the provided ID: {id}.");
            }
            return Ok(u);
        }



        /// <summary>
        /// Deletes an existing movimentos.
        /// </summary>
        /// <param name="id">The movimentos identifier.</param>
        /// <remarks>
        /// Deletes the movimentos identified by <paramref name="id"/>.
        /// </remarks>
        /// <response code="204">movimentos deleted successfully.</response>
        /// <response code="404">movimentos not found.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Delete(int id)
        {
            {
                _logger.LogInformation("CRUD {CrudOperation} {Resource} id={Id}", "Delete", "Cliente_Movimento", id);

                var DeleteMovimneto = _cliente_Movimento_Service.Delete(id);
                if (!DeleteMovimneto)
                {
                    return NotFound($"No Movimento exists with the provided ID: {id}.");
                }
                _logger.LogInformation($"Movimento with id => {id} was deleted.");
                return NoContent();
            }
        }
    }
}
