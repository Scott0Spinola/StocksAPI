using Microsoft.AspNetCore.Mvc;
using src.Dtos.Dashboard_Dtos;
using src.Services.Dashboard;

namespace src.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(DashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpPost("entradassaidas")]
    [ProducesResponseType(typeof(EntradasSaidasIndicadorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EntradasSaidasIndicadorResponse>> EntradasSaidas([FromBody] EntradasSaidasIndicadorRequest request)
    {
        _logger.LogInformation(
            "Dashboard entradas/saidas hotel={Hotel} tab={Tab} dataInicio={DataInicio} dataFim={DataFim}",
            request.Hotel,
            request.Tab,
            request.DataInicio,
            request.DataFim);

        var result = await _dashboardService.IndicadorEntradasSaidasAsync(request);
        return Ok(result);
    }

    [HttpPost("ultimasdescargas")]
    [ProducesResponseType(typeof(List<UltimasDescargasItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<UltimasDescargasItem>>> UltimasDescargas([FromBody] UltimasDescargasRequest request)
    {
        _logger.LogInformation(
            "Dashboard ultimasdescargas hotel={Hotel} tab={Tab} dataInicio={DataInicio} dataFim={DataFim} tipo={Tipo}",
            request.Hotel,
            request.Tab,
            request.DataInicio,
            request.DataFim,
            request.Tipo);

        var result = await _dashboardService.UltimasDescargasAsync(request);
        return Ok(result);
    }
}
