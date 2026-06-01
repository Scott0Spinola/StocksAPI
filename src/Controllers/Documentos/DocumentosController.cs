using Microsoft.AspNetCore.Mvc;
using src.Dtos.Documentos_Dtos;
using src.Services.Documentos;

namespace src.Controllers.Documentos;

[ApiController]
[Route("documentos")]

public class DocumentosController : ControllerBase
{
    private readonly DocumentosService _documentosService;
    private readonly ILogger<DocumentosController> _logger;

    public DocumentosController(DocumentosService documentosService, ILogger<DocumentosController> logger)
    {
        _documentosService = documentosService;
        _logger = logger;
    }

    [HttpPost("faturas/pesquisa")]
    [ProducesResponseType(typeof(List<DocumentoFaturaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DocumentoFaturaDTO>>> PesquisaFaturas([FromBody] FiltroFatura request)
    {
        _logger.LogInformation(
            "Pesquisa faturas hotel={Hotel} numDocumento={NumDocumento} dataInicio={DataInicio} dataFim={DataFim} pagina={Pagina} numRegistos={NumRegistos}",
            request.Hotel,
            request.NumDocumento,
            request.DataInicio,
            request.DataFim,
            request.Pagina,
            request.NumRegistos);

        var result = await _documentosService.PesquisaFaturasAsync(request);
        return Ok(result);
    }

    [HttpPost("guias/pesquisa")]
    [ProducesResponseType(typeof(List<DocumentoGuiaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DocumentoGuiaDTO>>> PesquisaGuias([FromBody] FiltroGuia request)
    {
        _logger.LogInformation(
            "Pesquisa guias hotel={Hotel} numDocumento={NumDocumento} dataInicio={DataInicio} dataFim={DataFim} pagina={Pagina} numRegistos={NumRegistos}",
            request.Hotel,
            request.NumDocumento,
            request.DataInicio,
            request.DataFim,
            request.Pagina,
            request.NumRegistos);

        var result = await _documentosService.PesquisaGuiasAsync(request);
        return Ok(result);
    }

    [HttpGet("download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(
        [FromQuery(Name = "tipo")] string? tipo,
        [FromQuery(Name = "docid")] Guid docId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return BadRequest("'tipo' is required (fatura|guia). Use /documentos/download?tipo=fatura&docid=<guid>.");
        }

        if (docId == Guid.Empty)
        {
            return BadRequest("'docid' is required.");
        }

        _logger.LogInformation("Documentos download tipo={Tipo} docid={DocId}", tipo, docId);

        var result = await _documentosService.DownloadAsync(tipo, docId, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }
        return File(result.Stream, result.ContentType, result.FileName);
    }
}
