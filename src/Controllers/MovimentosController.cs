using Microsoft.AspNetCore.Mvc;
using src.Dtos.Movimentos_Dtos;
using src.Services;

namespace src.Controllers;

[ApiController]
[Route("api/movimentos")]
public class MovimentosController : ControllerBase
{
    private readonly Cliente_Movimento_Services _movimentosService;
    private readonly ILogger<MovimentosController> _logger;

    public MovimentosController(Cliente_Movimento_Services movimentosService, ILogger<MovimentosController> logger)
    {
        _movimentosService = movimentosService;
        _logger = logger;
    }

    [HttpPost("pesquisa")]
    [ProducesResponseType(typeof(List<PesquisaItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PesquisaItem>>> Pesquisa([FromBody] PesquisaRequest request)
    {
        _logger.LogInformation(
            "Pesquisa movimentos hotel={Hotel} dataInicio={DataInicio} dataFim={DataFim} tipo={Tipo} numGuia={NumGuia}",
            request.Hotel,
            request.DataInicio,
            request.DataFim,
            request.Tipo,
            request.NumGuia);

        var result = await _movimentosService.PesquisaAsync(request);
        return Ok(result);
    }
}
