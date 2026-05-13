using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Stocks_Dtos;
using src.Models;
using src.Services.StocksServices;
using Xunit;

namespace src.Tests;

public class StocksRentingTests
{
    private static StocksServices CreateService(out StocksContext context)
    {
        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(StocksRentingTests)}_{Guid.NewGuid()}")
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new StocksServices(context, NullLogger<StocksServices>.Instance);
    }

    [Fact]
    public async Task PesquisaRentingAsync_GroupsByProduto_AndSplitsBy60Days()
    {
        var service = CreateService(out var context);

        var referenceDate = new DateTime(2026, 5, 13);
        var cutoff = referenceDate.AddDays(-60);

        context.Cliente_Tags.AddRange(
            // HotelA / Produto=Lençol: 2 total -> 1 <= cutoff, 1 > cutoff
            new Cliente_Tag { Unidade = "HotelA", Produto = "Lencol", EPC = "E1", Data_Ultimo_Movimento = cutoff },
            new Cliente_Tag { Unidade = "HotelA", Produto = "Lencol", EPC = "E2", Data_Ultimo_Movimento = cutoff.AddDays(1) },

            // HotelA / Produto=Toalha: 3 total -> 2 <= cutoff, 1 > cutoff
            new Cliente_Tag { Unidade = "HotelA", Produto = "Toalha", EPC = "T1", Data_Ultimo_Movimento = cutoff.AddDays(-1) },
            new Cliente_Tag { Unidade = "HotelA", Produto = "Toalha", EPC = "T2", Data_Ultimo_Movimento = cutoff.AddDays(-10) },
            new Cliente_Tag { Unidade = "HotelA", Produto = "Toalha", EPC = "T3", Data_Ultimo_Movimento = cutoff.AddDays(10) },

            // Noise: other hotel
            new Cliente_Tag { Unidade = "HotelB", Produto = "Lencol", EPC = "N1", Data_Ultimo_Movimento = cutoff.AddDays(-100) }
        );

        await context.SaveChangesAsync();

        var result = await service.PesquisaRentingAsync(new FiltroStock(Hotel: "HotelA"), referenceDateOverride: referenceDate);

        Assert.Equal(2, result.Count);

        var lencol = Assert.Single(result, x => x.Produto == "Lencol");
        Assert.Equal(2, lencol.Qtd);
        Assert.Equal(1, lencol.QtdMais60dias);
        Assert.Equal(1, lencol.QtdMenos60dias);

        var toalha = Assert.Single(result, x => x.Produto == "Toalha");
        Assert.Equal(3, toalha.Qtd);
        Assert.Equal(2, toalha.QtdMais60dias);
        Assert.Equal(1, toalha.QtdMenos60dias);

        // Ordered by Produto.
        Assert.Equal(new[] { "Lencol", "Toalha" }, result.Select(x => x.Produto).ToArray());
    }

    [Fact]
    public async Task DetalheRentingAsync_FiltersByHotelAndProduto_ReturnsEpcAndDateOrderedDesc()
    {
        var service = CreateService(out var context);

        var d1 = new DateTime(2026, 5, 1);
        var d2 = new DateTime(2026, 5, 10);

        context.Cliente_Tags.AddRange(
            new Cliente_Tag { Unidade = "HotelA", Produto = "Lencol", EPC = "E1", Data_Ultimo_Movimento = d1 },
            new Cliente_Tag { Unidade = "HotelA", Produto = "Lencol", EPC = "E2", Data_Ultimo_Movimento = d2 },
            // Noise: other product
            new Cliente_Tag { Unidade = "HotelA", Produto = "Toalha", EPC = "T1", Data_Ultimo_Movimento = d2 },
            // Noise: other hotel
            new Cliente_Tag { Unidade = "HotelB", Produto = "Lencol", EPC = "N1", Data_Ultimo_Movimento = d2 }
        );

        await context.SaveChangesAsync();

        var result = await service.DetalheRentingAsync(new FiltroProdutoStock(Hotel: "HotelA", TipoProduto: "Lencol"));

        Assert.Equal(2, result.Count);
        Assert.Equal("E2", result[0].Rfid);
        Assert.Equal(d2, result[0].DataMovimento);
        Assert.Equal("E1", result[1].Rfid);
        Assert.Equal(d1, result[1].DataMovimento);
    }
}
