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

        public Cliente_Tag_Controller(Cliente_Tag_Services cliente_Tag_Services, ILogger<Cliente_Tag_Controller> logger)
        {
            _Tag_Servicese = cliente_Tag_Services;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(PagedList<Cliente_Tag>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedList<Cliente_Tag>>> GetAll([FromQuery] PageParameters pageParameters)
        {

            var paged = await _Tag_Servicese.GetAllPaged(pageParameters);
            return Ok(paged);
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PagedList<Cliente_Movimento>), StatusCodes.Status200OK)]
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


        [HttpPost]
        [ProducesResponseType(typeof(Cliente_Tag), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Cliente_Tag>> CreateTag([FromBody] CreateTag dto)
        {
            var created = await _Tag_Servicese.CreateTag(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

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
