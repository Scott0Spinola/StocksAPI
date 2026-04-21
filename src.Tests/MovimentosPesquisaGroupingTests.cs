using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Movimentos_Dtos;
using src.Models;
using src.Services.EntradasSaidasService;
using Xunit;

namespace src.Tests;

public class MovimentosPesquisaGroupingTests
{
    private static EntradasSaidasService CreateService(out StocksContext context)
    {
        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(MovimentosPesquisaGroupingTests)}_{Guid.NewGuid()}")
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new EntradasSaidasService(context, NullLogger<EntradasSaidasService>.Instance);
    }

    [Fact]
    public async Task Tab0_Dia_Returns24HoursWithZeros()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);
        var d1 = new DateTime(2026, 4, 14, 10, 0, 0);
        var d2 = new DateTime(2026, 4, 14, 12, 0, 0);
        var d3 = new DateTime(2026, 4, 14, 11, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Descricao = "Desc E1", Datetime = d1, Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = d2, Quantidade = 3 },
            new Cliente_Movimento { MovementRID = "S1", Para = "Lavandaria", De = "HotelA", Datetime = d3, Quantidade = 4 }
        );

        await context.SaveChangesAsync();

        var request = new PesquisaRequest(
            Hotel: "HotelA",
            Tab: 0,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 2);

        var result = await service.PesquisaAsync(request);

        Assert.Equal(48, result.Count);

        Assert.Equal(new DateTime(2026, 4, 14, 0, 0, 0), result.First().Data);
        Assert.Equal(new DateTime(2026, 4, 14, 23, 0, 0), result.Last().Data);

        var h10Entrada = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 14, 10, 0, 0) && r.Direcao == 0);
        Assert.Equal(2, h10Entrada.Qtd);
        Assert.Equal("Desc E1", h10Entrada.Produto);

        var h11Saida = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 14, 11, 0, 0) && r.Direcao == 1);
        Assert.Equal(4, h11Saida.Qtd);

        var h9Entrada = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 14, 9, 0, 0) && r.Direcao == 0);
        Assert.Equal(0, h9Entrada.Qtd);
    }

    [Fact]
    public async Task Tab0_Dia_PaginatesAfterSortingByDateAndDirection()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);
        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 10, 0, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "S1", Para = "Lavandaria", De = "HotelA", Datetime = new DateTime(2026, 4, 14, 11, 0, 0), Quantidade = 4 }
        );

        await context.SaveChangesAsync();

        var request = new PesquisaRequest(
            Hotel: "HotelA",
            Tab: 0,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 2,
            Pagina: 2,
            NumRegistos: 10);

        var result = await service.PesquisaAsync(request);

        Assert.Equal(10, result.Count);
        Assert.Equal(new DateTime(2026, 4, 14, 5, 0, 0), result.First().Data);
        Assert.Equal(0, result.First().Direcao);
        Assert.Equal(new DateTime(2026, 4, 14, 9, 0, 0), result.Last().Data);
        Assert.Equal(1, result.Last().Direcao);
    }

    [Fact]
    public async Task Tab1_Semana_Returns7DaysEndingAtAnchor()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);
        var withinWindow1 = new DateTime(2026, 4, 10, 10, 0, 0);
        var withinWindow2 = new DateTime(2026, 4, 14, 9, 0, 0);
        var outsideWindow = new DateTime(2026, 4, 7, 8, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = withinWindow1, Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = withinWindow2, Quantidade = 5 },
            new Cliente_Movimento { MovementRID = "E3", Para = "HotelA", De = "Lavandaria", Datetime = outsideWindow, Quantidade = 99 }
        );

        await context.SaveChangesAsync();

        var request = new PesquisaRequest(
            Hotel: "HotelA",
            Tab: 1,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 0);

        var result = await service.PesquisaAsync(request);

        Assert.Equal(7, result.Count);
        Assert.Equal(new DateTime(2026, 4, 8), result.First().Data);
        Assert.Equal(new DateTime(2026, 4, 14), result.Last().Data);

        var d10 = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 10));
        Assert.Equal(2, d10.Qtd);

        var d12 = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 12));
        Assert.Equal(0, d12.Qtd);
    }

    [Fact]
    public async Task Tab2_Mes_Returns31DaysEndingAtAnchor()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 12, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 3, 20, 10, 0, 0), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 10, 0, 0), Quantidade = 2 }
        );

        await context.SaveChangesAsync();

        var request = new PesquisaRequest(
            Hotel: "HotelA",
            Tab: 2,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 0);

        var result = await service.PesquisaAsync(request);

        Assert.Equal(31, result.Count);
        Assert.Equal(new DateTime(2026, 3, 15), result.First().Data);
        Assert.Equal(new DateTime(2026, 4, 14), result.Last().Data);

        var mar20 = Assert.Single(result, r => r.Data == new DateTime(2026, 3, 20));
        Assert.Equal(1, mar20.Qtd);

        var apr14 = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 14));
        Assert.Equal(2, apr14.Qtd);
    }

    [Fact]
    public async Task Tab3_Ano_Returns12MonthsEndingAtAnchorMonth()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 12, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2025, 5, 10, 10, 0, 0), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 1, 10, 0, 0), Quantidade = 2 }
        );

        await context.SaveChangesAsync();

        var request = new PesquisaRequest(
            Hotel: "HotelA",
            Tab: 3,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 0);

        var result = await service.PesquisaAsync(request);

        Assert.Equal(12, result.Count);
        Assert.Equal(new DateTime(2025, 5, 1), result.First().Data);
        Assert.Equal(new DateTime(2026, 4, 1), result.Last().Data);

        var may2025 = Assert.Single(result, r => r.Data == new DateTime(2025, 5, 1));
        Assert.Equal(1, may2025.Qtd);

        var apr2026 = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 1));
        Assert.Equal(2, apr2026.Qtd);
    }

    [Fact]
    public async Task Tab4_Personalizado_UsesInputDateRange()
    {
        var service = CreateService(out var context);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 10, 0, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 15, 10, 0, 0), Quantidade = 3 },
            new Cliente_Movimento { MovementRID = "S1", Para = "Lavandaria", De = "HotelA", Datetime = new DateTime(2026, 4, 15, 11, 0, 0), Quantidade = 7 }
        );

        await context.SaveChangesAsync();

        var request = new PesquisaRequest(
            Hotel: "HotelA",
            Tab: 4,
            DataInicio: new DateTime(2026, 4, 14, 0, 0, 0),
            DataFim: new DateTime(2026, 4, 15, 23, 59, 59),
            Tipo: 2);

        var result = await service.PesquisaAsync(request);

        Assert.Equal(4, result.Count);

        Assert.Equal(new DateTime(2026, 4, 14), result.First().Data);
        Assert.Equal(new DateTime(2026, 4, 15), result.Last().Data);

        var d14Entradas = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 14) && r.Direcao == 0);
        Assert.Equal(2, d14Entradas.Qtd);

        var d14Saidas = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 14) && r.Direcao == 1);
        Assert.Equal(0, d14Saidas.Qtd);

        var d15Entradas = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 15) && r.Direcao == 0);
        Assert.Equal(3, d15Entradas.Qtd);

        var d15Saidas = Assert.Single(result, r => r.Data == new DateTime(2026, 4, 15) && r.Direcao == 1);
        Assert.Equal(7, d15Saidas.Qtd);
    }
}
