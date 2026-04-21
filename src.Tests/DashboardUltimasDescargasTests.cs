using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Dashboard_Dtos;
using src.Models;
using src.Services.Dashboard;
using Xunit;

namespace src.Tests;

public class DashboardUltimasDescargasTests
{
    private static DashboardService CreateService(out StocksContext context)
    {
        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(DashboardUltimasDescargasTests)}_{Guid.NewGuid()}")
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();

        return new DashboardService(context, NullLogger<DashboardService>.Instance);
    }

    [Fact]
    public async Task Tab0_Dia_Tipo2_GroupsByHourAndUnionsEntradasSaidas()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 10, 15, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 10, 59, 0), Quantidade = 3 },
            new Cliente_Movimento { MovementRID = "S1", Para = "Lavandaria", De = "HotelA", Datetime = new DateTime(2026, 4, 14, 11, 0, 0), Quantidade = 4 },
            // Noise
            new Cliente_Movimento { MovementRID = "S2", Para = "Lavandaria", De = "HotelB", Datetime = new DateTime(2026, 4, 14, 11, 0, 0), Quantidade = 999 }
        );

        await context.SaveChangesAsync();

        var request = new UltimasDescargasRequest(
            Hotel: "HotelA",
            Tab: 0,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 2);

        var result = await service.UltimasDescargasAsync(request);

        Assert.Equal(48, result.Count);

        var first = result.First();
        Assert.Equal("14/04", first.Data);
        Assert.Equal("00", first.Hora);
        Assert.Equal(0, first.Direcao);

        var h10Entrada = Assert.Single(result, r => r.Data == "14/04" && r.Hora == "10" && r.Direcao == 0);
        Assert.Equal(5, h10Entrada.Qtd);

        var h11Saida = Assert.Single(result, r => r.Data == "14/04" && r.Hora == "11" && r.Direcao == 1);
        Assert.Equal(4, h11Saida.Qtd);

        var h9Entrada = Assert.Single(result, r => r.Data == "14/04" && r.Hora == "09" && r.Direcao == 0);
        Assert.Equal(0, h9Entrada.Qtd);
    }

    [Fact]
    public async Task Tab1_Semana_Tipo0_GroupsByDay()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "RID-1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 10, 10, 0, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "RID-2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 10, 12, 0, 0), Quantidade = 5 },
            new Cliente_Movimento { MovementRID = "RID-1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 11, 9, 0, 0), Quantidade = 1 }
        );

        await context.SaveChangesAsync();

        var request = new UltimasDescargasRequest(
            Hotel: "HotelA",
            Tab: 1,
            DataInicio: anchor,
            DataFim: anchor,
            Tipo: 0);

        var result = await service.UltimasDescargasAsync(request);

        Assert.Equal(7, result.Count);
        Assert.All(result, r => Assert.Equal(0, r.Direcao));
        Assert.All(result, r => Assert.Equal(string.Empty, r.Hora));

        Assert.Equal("08/04", result.First().Data);
        Assert.Equal("14/04", result.Last().Data);

        var d10 = Assert.Single(result, r => r.Data == "10/04");
        Assert.Equal(7, d10.Qtd);

        var d11 = Assert.Single(result, r => r.Data == "11/04");
        Assert.Equal(1, d11.Qtd);

        var d12 = Assert.Single(result, r => r.Data == "12/04");
        Assert.Equal(0, d12.Qtd);
    }
}
