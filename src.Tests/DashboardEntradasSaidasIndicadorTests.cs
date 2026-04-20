using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Dashboard_Dtos;
using src.Models;
using src.Services.Dashboard;
using Xunit;

namespace src.Tests;

public class DashboardEntradasSaidasIndicadorTests
{
    private static DashboardService CreateService(out StocksContext context)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseSqlite(connection)
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new DashboardService(context, NullLogger<DashboardService>.Instance);
    }

    [Fact]
    public async Task Tab1_Semana_ComputaPeriodoAnterior()
    {
        var service = CreateService(out var context);

        var anchor = new DateTime(2026, 4, 14, 15, 0, 0);

        // Janela atual (tab=1): 08/04 a 14/04.
        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "C-E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 10, 10, 0, 0), Quantidade = 2 },
            new Cliente_Movimento { MovementRID = "C-E2", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 14, 9, 0, 0), Quantidade = 5 },
            new Cliente_Movimento { MovementRID = "C-S1", Para = "Lavandaria", De = "HotelA", Datetime = new DateTime(2026, 4, 12, 11, 0, 0), Quantidade = 3 }
        );

        // Janela anterior: 01/04 a 07/04.
        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "P-E1", Para = "HotelA", De = "Lavandaria", Datetime = new DateTime(2026, 4, 2, 10, 0, 0), Quantidade = 7 },
            new Cliente_Movimento { MovementRID = "P-S1", Para = "Lavandaria", De = "HotelA", Datetime = new DateTime(2026, 4, 3, 11, 0, 0), Quantidade = 11 }
        );

        // Noise.
        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "N-E", Para = "HotelB", De = "Lavandaria", Datetime = new DateTime(2026, 4, 10, 10, 0, 0), Quantidade = 999 },
            new Cliente_Movimento { MovementRID = "N-S", Para = "Lavandaria", De = "HotelB", Datetime = new DateTime(2026, 4, 3, 11, 0, 0), Quantidade = 999 }
        );

        await context.SaveChangesAsync();

        var request = new EntradasSaidasIndicadorRequest(
            Hotel: "HotelA",
            Tab: 1,
            DataInicio: anchor,
            DataFim: anchor);

        var result = await service.IndicadorEntradasSaidasAsync(request);

        Assert.Equal(7, result.Entradas.NumPecas);
        Assert.Equal(7, result.Entradas.NumPecasAnterior);

        Assert.Equal(3, result.Saidas.NumPecas);
        Assert.Equal(11, result.Saidas.NumPecasAnterior);

        Assert.Equal(3 - 7, result.Diferenca.NumPecas);
        Assert.Equal(11 - 7, result.Diferenca.NumPecasAnterior);

        Assert.Equal(0m, result.Entradas.Peso);
        Assert.Equal(0m, result.Saidas.Peso);
        Assert.Equal(0m, result.Diferenca.Peso);
    }
}
