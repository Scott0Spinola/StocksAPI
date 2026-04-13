using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Movimentos_Dtos;
using src.Models;
using src.Services;
using Xunit;

namespace src.Tests;

public class MovimentosProximasEntregasTests
{
    private static Cliente_Movimento_Services CreateService(out StocksContext context)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseSqlite(connection)
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new Cliente_Movimento_Services(context, NullLogger<Cliente_Movimento_Services>.Instance);
    }

    [Fact]
    public async Task Tipo0_Entradas_ReturnsNextDeliveryPerHotel()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "A-1", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(2), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "A-2", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "B-1", Para = "HotelB", De = "Lavandaria", Datetime = now.AddHours(3), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "PAST", Para = "HotelA", De = "Lavandaria", Datetime = now.AddDays(-1), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "todas-unidades",
            DataInicio: now.AddDays(-2),
            DataFim: now.AddDays(2),
            NumGuia: null,
            Tipo: 0,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request, hotelQuery: "todas-unidades");

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.UnidadeHotel == "HotelA");
        Assert.Contains(result, r => r.UnidadeHotel == "HotelB");

        var hotelA = result.Single(r => r.UnidadeHotel == "HotelA");
        Assert.Equal(now.AddHours(1).ToString("dd/MM/yyyy"), hotelA.DataPrevista);
    }

    [Fact]
    public async Task Tipo1_Saidas_UsesDeAsHotelAndParaLavandaria()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "S-1", De = "HotelA", Para = "Lavandaria", Datetime = now.AddMinutes(30), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "S-2", De = "HotelA", Para = "Lavandaria", Datetime = now.AddHours(2), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "NOT-S", De = "HotelA", Para = "HotelA", Datetime = now.AddMinutes(10), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "HotelA",
            DataInicio: now.AddDays(-1),
            DataFim: now.AddDays(1),
            NumGuia: null,
            Tipo: 1,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request, hotelQuery: "HotelA");

        Assert.Single(result);
        var item = result[0];
        Assert.Equal("HotelA", item.UnidadeHotel);
        Assert.Equal(now.AddMinutes(30).ToString("HH:mm"), item.HoraPrevista);
    }

    [Fact]
    public async Task Tipo2_Ambos_PicksEarliestOfEntryOrExitPerHotel()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "ENT-A", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "SAI-A", De = "HotelA", Para = "Lavandaria", Datetime = now.AddMinutes(20), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "ENT-B", Para = "HotelB", De = "Lavandaria", Datetime = now.AddMinutes(50), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "todas-unidades",
            DataInicio: now.AddDays(-1),
            DataFim: now.AddDays(1),
            NumGuia: null,
            Tipo: 2,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request, hotelQuery: "todas-unidades");

        Assert.Equal(2, result.Count);
        var hotelA = result.Single(r => r.UnidadeHotel == "HotelA");
        Assert.Equal(now.AddMinutes(20).ToString("HH:mm"), hotelA.HoraPrevista);
    }
}
