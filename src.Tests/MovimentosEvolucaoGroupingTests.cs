using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Movimentos_Dtos;
using src.Models;
using src.Services.EntradasSaidasService;
using Xunit;

namespace src.Tests;

public class MovimentosEvolucaoGroupingTests
{
    private static EntradasSaidasService CreateService(out StocksContext context)
    {
        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(MovimentosEvolucaoGroupingTests)}_{Guid.NewGuid()}")
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new EntradasSaidasService(context, NullLogger<EntradasSaidasService>.Instance);
    }

    [Fact]
    public async Task Tab0_Dia_Tipo2_Returns24HoursPerDirectionWithZeros()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Descricao = "Desc E1", Datetime = new DateTime(2026, 4, 14, 10, 0, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 12, 0, 0), Quantidade = 3 },
            new Cliente_Movimento { MovementRID = "S1", Para = "Lavandaria", De = "HotelA", Datetime = new DateTime(2026, 4, 14, 11, 0, 0), Quantidade = 4 },
            // Noise: a different hotel's exit should not count for HotelA.
            new Cliente_Movimento { MovementRID = "S2", Para = "Lavandaria", De = "HotelB", Datetime = new DateTime(2026, 4, 14, 11, 0, 0), Quantidade = 999 }
        );

        await context.SaveChangesAsync();

        var request = new EvolucaoRequest(
            Hotel: "HotelA",
            Tab: 0,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 2);

        var result = await service.EvolucaoAsync(request);

        Assert.Equal(48, result.Count);

        var first = result.First();
        Assert.Equal("14/04", first.Data);
        Assert.Equal("00", first.Hora);
        Assert.Equal(0, first.Direcao);

        var h10Entrada = Assert.Single(result, r => r.Data == "14/04" && r.Hora == "10" && r.Direcao == 0);
        Assert.Equal(2, h10Entrada.Qtd);

        var h11Saida = Assert.Single(result, r => r.Data == "14/04" && r.Hora == "11" && r.Direcao == 1);
        Assert.Equal(4, h11Saida.Qtd);

        var h9Entrada = Assert.Single(result, r => r.Data == "14/04" && r.Hora == "09" && r.Direcao == 0);
        Assert.Equal(0, h9Entrada.Qtd);
    }

    [Fact]
    public async Task Tab1_Semana_Tipo0_Returns7DaysEndingAtAnchor()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 10, 10, 0, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 9, 0, 0), Quantidade = 5 },
            new Cliente_Movimento { MovementRID = "E3", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 7, 8, 0, 0), Quantidade = 99 }
        );

        await context.SaveChangesAsync();

        var request = new EvolucaoRequest(
            Hotel: "HotelA",
            Tab: 1,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 0
            );

        var result = await service.EvolucaoAsync(request);

        Assert.Equal(7, result.Count);
        Assert.Equal("08/04", result.First().Data);
        Assert.Equal("14/04", result.Last().Data);

        var d10 = Assert.Single(result, r => r.Data == "10/04" && r.Direcao == 0);
        Assert.Equal(2, d10.Qtd);

        var d12 = Assert.Single(result, r => r.Data == "12/04" && r.Direcao == 0);
        Assert.Equal(0, d12.Qtd);
    }

    [Fact]
    public async Task Tab3_Ano_Tipo0_Returns12MonthsEndingAtAnchorMonth()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 12, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2025, 5, 10, 10, 0, 0), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 1, 10, 0, 0), Quantidade = 2 }
        );

        await context.SaveChangesAsync();

        var request = new EvolucaoRequest(
            Hotel: "HotelA",
            Tab: 3,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 0
           );

        var result = await service.EvolucaoAsync(request);

        Assert.Equal(12, result.Count);
        Assert.Equal("05/2025", result.First().Data);
        Assert.Equal("04/2026", result.Last().Data);

        var may2025 = Assert.Single(result, r => r.Data == "05/2025" && r.Direcao == 0);
        Assert.Equal(1, may2025.Qtd);

        var apr2026 = Assert.Single(result, r => r.Data == "04/2026" && r.Direcao == 0);
        Assert.Equal(2, apr2026.Qtd);
    }
}
