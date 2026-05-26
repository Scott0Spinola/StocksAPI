using Microsoft.AspNetCore.Mvc;
using src.Dtos.Faturacao_Dtos;
using src.Services.Faturacao;

namespace src.Controllers.Faturacao;

/// <summary>
/// Endpoints for faturação indicators.
/// </summary>
[ApiController]
[Route("faturacao")]
public class FaturacaoController : ControllerBase
{
    private readonly FaturacaoService _faturacaoService;
    private readonly ILogger<FaturacaoController> _logger;

    public FaturacaoController(FaturacaoService faturacaoService, ILogger<FaturacaoController> logger)
    {
        _faturacaoService = faturacaoService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates faturação indicators for a given period (inclusive), including IVA and credit notes,
    /// and compares the total against the previous period.
    /// </summary>
    /// <remarks>
    /// Previous period window is computed as the immediately preceding window with the same duration.
    /// </remarks>
    /// <param name="request">Filter containing hotel and date window.</param>
    /// <returns>Computed faturação indicators.</returns>
    [HttpPost("indicadores")]
    [ProducesResponseType(typeof(IndicadoresFaturacaoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IndicadoresFaturacaoDTO>> Indicadores([FromBody] FiltroFaturacao request)
    {
        _logger.LogInformation(
            "Faturacao indicadores hotel={Hotel} dataInicio={DataInicio} dataFim={DataFim}",
            request.Hotel,
            request.DataInicio,
            request.DataFim);

        var result = await _faturacaoService.IndicadoresAsync(request);
        return Ok(result);
    }
}
