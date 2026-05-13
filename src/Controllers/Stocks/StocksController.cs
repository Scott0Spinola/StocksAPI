using Microsoft.AspNetCore.Mvc;
using src.Dtos.Stocks_Dtos;
using src.Services.StocksServices;

namespace src.Controllers.Stocks;

[ApiController]
[Route("api/stocks")]
public class StocksController : ControllerBase
{
    private readonly StocksServices _stocksServices;
    private readonly ILogger<StocksController> _logger;

    public StocksController(StocksServices stocksServices, ILogger<StocksController> logger)
    {
        _stocksServices = stocksServices;
        _logger = logger;
    }

    [HttpPost("renting/pesquisa")]
    [ProducesResponseType(typeof(List<TipoProdutoStockDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TipoProdutoStockDTO>>> PesquisaRenting([FromBody] FiltroStock request)
    {
        _logger.LogInformation("Stocks renting pesquisa hotel={Hotel}", request.Hotel);
        var result = await _stocksServices.PesquisaRentingAsync(request);
        return Ok(result);
    }

    [HttpPost("renting/detalhe")]
    [ProducesResponseType(typeof(List<ProdutoStockDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ProdutoStockDTO>>> DetalheRenting([FromBody] FiltroProdutoStock request)
    {
        _logger.LogInformation(
            "Stocks renting detalhe hotel={Hotel} produto={Produto}",
            request.Hotel,
            request.TipoProduto);

        var result = await _stocksServices.DetalheRentingAsync(request);
        return Ok(result);
    }
}
