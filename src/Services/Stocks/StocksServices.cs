
using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Dtos.Stocks_Dtos;

namespace src.Services.StocksServices;

public class StocksServices
{
	private readonly StocksContext _context;
	private readonly ILogger<StocksServices> _logger;

	public StocksServices(StocksContext context, ILogger<StocksServices> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<List<TipoProdutoStockDTO>> PesquisaRentingAsync(FiltroStock filtro, DateTime? referenceDateOverride = null)
	{
		if (string.IsNullOrWhiteSpace(filtro.Hotel))
		{
			throw new ArgumentException("'hotel' is required.");
		}

		var hotel = filtro.Hotel.Trim();
		var referenceDate = (referenceDateOverride ?? DateTime.UtcNow).Date;
		var cutoffDate = referenceDate.AddDays(-60);

		var aggregates = await _context.Cliente_Tags
			.AsNoTracking()
			.Where(t => t.Unidade == hotel)
			.GroupBy(t => t.Produto)
			.Select(g => new
			{
				Produto = g.Key ?? string.Empty,
				Qtd = g.Count(),
				QtdMais60dias = g.Sum(x => x.Data_Ultimo_Movimento <= cutoffDate ? 1 : 0),
				QtdMenos60dias = g.Sum(x => x.Data_Ultimo_Movimento > cutoffDate ? 1 : 0)
			})
			.OrderBy(x => x.Produto)
			.ToListAsync();

		var result = aggregates
			.Select(x => new TipoProdutoStockDTO(
				Produto: x.Produto,
				Qtd: x.Qtd,
				QtdMais60dias: x.QtdMais60dias,
				QtdMenos60dias: x.QtdMenos60dias))
			.ToList();

		_logger.LogInformation(
			"Stocks renting pesquisa hotel={Hotel} cutoff={Cutoff} grupos={Grupos}",
			hotel,
			cutoffDate,
			result.Count);

		return result;
	}

	public async Task<List<ProdutoStockDTO>> DetalheRentingAsync(FiltroProdutoStock filtro)
	{
		if (string.IsNullOrWhiteSpace(filtro.Hotel))
		{
			throw new ArgumentException("'hotel' is required.");
		}

		if (string.IsNullOrWhiteSpace(filtro.TipoProduto))
		{
			throw new ArgumentException("'tipoProduto' is required.");
		}

		var hotel = filtro.Hotel.Trim();
		var tipoProduto = filtro.TipoProduto.Trim();

		var result = await _context.Cliente_Tags
			.AsNoTracking()
			.Where(t => t.Unidade == hotel)
			.Where(t => t.Produto == tipoProduto)
			.Select(t => new ProdutoStockDTO(
				Rfid: t.EPC ?? string.Empty,
				DataMovimento: t.Data_Ultimo_Movimento))
			.OrderByDescending(x => x.DataMovimento)
			.ToListAsync();

		_logger.LogInformation(
			"Stocks renting detalhe hotel={Hotel} produto={Produto} itens={Itens}",
			hotel,
			tipoProduto,
			result.Count);

		return result;
	}
}