using IntervencoesAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using src.Dtos.Cliente_Tag_Dtos;
using src.Models;
using src.Services;

namespace src.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Cliente_Tag_Controller : ControllerBase
    {
        private readonly Cliente_Tag_Services _Tag_Servicese;
        private readonly ILogger<Cliente_Tag_Controller> _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="Cliente_Tag_Controller"/> class.
        /// </summary>
        /// <param name="cliente_Tag_Services">The service layer for tag operations.</param>
        /// <param name="logger">The logger instance for request logging.</param>
        /// <remarks>
        /// Injects the required dependencies for tag service operations and structured logging.
        /// Assigns the service to the backing field <c>_Tag_Servicese</c> and logger to <c>_logger</c>.
        /// </remarks>
        public Cliente_Tag_Controller(Cliente_Tag_Services cliente_Tag_Services, ILogger<Cliente_Tag_Controller> logger)
        {
            _Tag_Servicese = cliente_Tag_Services;
            _logger = logger;
        }

        /// <summary>
        /// Gets a paged list of all tags.
        /// </summary>
        /// <param name="pageParameters">The pagination and sorting parameters.</param>
        /// <remarks>
        /// Retrieves a paged collection of <see cref="Cliente_Tag"/> resources according to the supplied pagination settings.
        /// </remarks>
        /// <response code="200">Paged tags returned successfully.</response>
        /// <response code="404">No tags found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedList<Cliente_Tag>>> GetAll([FromQuery] PageParameters pageParameters)
        {

            var paged = await _Tag_Servicese.GetAllPaged(pageParameters);
            return Ok(paged);
        }



        /// <summary>
        /// Gets a tag by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tag.</param>
        /// <remarks>
        /// Retrieves a <see cref="Cliente_Tag"/> resource by its identifier.
        /// Returns 404 if the tag does not exist.
        /// </remarks>
        /// <response code="200">Tag returned successfully.</response>
        /// <response code="404">Tag not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cliente_Tag> GetById(int id)
        {
            var t = _Tag_Servicese.GetById(id);
            if (t is null)
            {
                return NotFound($"No Tag exists with the provided Id: {id}. ");
            }
            return t;
        }





        [HttpGet("Unidade")]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cliente_Tag> GetByUnidade(string unidade)
        {
            var t = _Tag_Servicese.GetByUnidade(unidade);
            if (t is null)
            {
                return NotFound($"No Tag exists with the provided Unidade: {unidade}. ");
            }
            return t;
        }

        [HttpGet("Localizacao")]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cliente_Tag> GetByLocalizacao(string localizacao)
        {
            var t = _Tag_Servicese.GetByLocalizacao(localizacao);
            if (t is null)
            {
                return NotFound($"No Tag exists with the provided Localização: {localizacao}. ");
            }
            return t;
        }



        [HttpGet("Produto")]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cliente_Tag> GetByProduto(string produto)
        {
            var t = _Tag_Servicese.GetByProduto(produto);
            if (t is null)
            {
                return NotFound($"No Tag exists with the provided Produto: {produto}. ");
            }
            return t;
        }
        

        [HttpGet("EPC")]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cliente_Tag> GetByEPC(string epc)
        {
            var t = _Tag_Servicese.GetByEPC(epc);
            if (t is null)
            {
                return NotFound($"No Tag exists with the provided EPC: {epc}. ");
            }
            return t;
        }



        [HttpGet("Estado")]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Cliente_Tag> GetByEstado(string estado)
        {
            var t = _Tag_Servicese.GetByEstado(estado);
            if (t is null)
            {
                return NotFound($"No Tag exists with the provided estado: {estado}. ");
            }
            return t;
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <param name="dto">The data used to create the new tag from the request body.</param>
        /// <remarks>
        /// Calls the service layer to create a new <see cref="Cliente_Tag"/> resource and returns 201 Created with the created entity
        /// and a location header pointing to the new resource's GetById endpoint.
        /// </remarks>
        /// <response code="201">Tag created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Cliente_Tag), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Cliente_Tag>> CreateTag([FromBody] CreateTag dto)
        {
            var created = await _Tag_Servicese.CreateTag(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }



        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        /// <param name="id">The unique identifier of the tag to update.</param>
        /// <param name="dto">The updated data for the tag from the request body.</param>
        /// <remarks>
        /// Calls the service layer to update the <see cref="Cliente_Tag"/> resource and returns the updated entity on success,
        /// or 404 if the resource is not found.
        /// </remarks>
        /// <response code="200">Tag updated successfully.</response>
        /// <response code="404">Tag not found.</response>
        /// <response code="400">Invalid request.</response>
        [HttpPut]
        [ProducesResponseType(typeof(Cliente_Tag), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<Cliente_Tag>> UpdateTag(int id, [FromBody] UpdateTag dto)
        {
            var u = await _Tag_Servicese.Update(id, dto);


            if (u is null)
            {
                return NotFound($"No Tag exists with the provided ID: {id}.");
            }
            return Ok(u);
        }



        /// <summary>
        /// Deletes a tag by identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tag to delete.</param>
        /// <response code="204">Tag deleted successfully.</response>
        /// <response code="404">Tag not found.</response>
        /// <response code="400">Invalid request.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Delete(int id)
        {
            var d = _Tag_Servicese.Delete(id);

            if (!d)
            {
                return NotFound($"No Tag exists with the provided ID: {id}.");
            }
            return NoContent();
        }
    }
}
