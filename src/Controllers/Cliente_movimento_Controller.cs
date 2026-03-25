using IntervencoesAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        /// Gets a movimento by identifier.
        /// </summary>
        /// <param name="id">The movimento identifier.</param>
        /// <remarks>
        /// Returns the movimento resource if found.
        /// </remarks>
        /// <response code="200">Movimento returned successfully.</response>
        /// <response code="404">Movimento not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PagedList<Cliente_Movimento>), StatusCodes.Status200OK)]
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
    }
}
