using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Movimentos_Dtos;
using src.Models;
using src.Services;
using src.Services.EntradasSaidasService;
using Xunit;

namespace src.Tests;

public class MovimentosProximasEntregasTests
{
    private static EntradasSaidasService CreateService(out StocksContext context)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseSqlite(connection)
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new EntradasSaidasService(context, NullLogger<EntradasSaidasService>.Instance);
    }

    [Fact]
    public async Task ScopesResultsByClienteFromBody()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "C1-A", Cliente = "Cliente1", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "C1-B", Cliente = "Cliente1", De = "HotelA", Para = "Lavandaria", Datetime = now.AddHours(2), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "C2-A", Cliente = "Cliente2", Para = "HotelX", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "Cliente1",
            DataInicio: now.AddDays(-1),
            DataFim: now.AddDays(1),
            Tipo: 2,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.UnidadeHotel == "HotelX");
    }

    [Fact]
    public async Task Tipo0_Entradas_ReturnsUpcomingEntries()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "A-1", Cliente = "Cliente1", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(2), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "A-2", Cliente = "Cliente1", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "B-1", Cliente = "Cliente1", Para = "HotelB", De = "Lavandaria", Datetime = now.AddHours(3), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "PAST", Cliente = "Cliente1", Para = "HotelA", De = "Lavandaria", Datetime = now.AddDays(-1), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "OTHER", Cliente = "Cliente2", Para = "HotelZ", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "Cliente1",
            DataInicio: now.AddDays(-2),
            DataFim: now.AddDays(2),
            Tipo: 0,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request);

        Assert.Equal(3, result.Count);
        Assert.Equal("HotelA", result[0].UnidadeHotel);
        Assert.Equal(now.AddHours(1).ToString("HH:mm"), result[0].HoraPrevista);
        Assert.Equal("HotelA", result[1].UnidadeHotel);
        Assert.Equal(now.AddHours(2).ToString("HH:mm"), result[1].HoraPrevista);
        Assert.Equal("HotelB", result[2].UnidadeHotel);
    }

    [Fact]
    public async Task Tipo1_Saidas_UsesDeAsHotelAndParaLavandaria()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "S-1", Cliente = "Cliente1", De = "HotelA", Para = "Lavandaria", Datetime = now.AddMinutes(30), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "S-2", Cliente = "Cliente1", De = "HotelA", Para = "Lavandaria", Datetime = now.AddHours(2), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "S-3", Cliente = "Cliente1", De = "HotelB", Para = "Lavandaria", Datetime = now.AddMinutes(45), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "NOT-S", Cliente = "Cliente1", De = "HotelA", Para = "HotelA", Datetime = now.AddMinutes(10), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "OTHER", Cliente = "Cliente2", De = "HotelX", Para = "Lavandaria", Datetime = now.AddMinutes(40), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "Cliente1",
            DataInicio: now.AddDays(-1),
            DataFim: now.AddDays(1),
            Tipo: 1,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request);

        Assert.Equal(3, result.Count);
        Assert.Equal("HotelA", result[0].UnidadeHotel);
        Assert.Equal(now.AddMinutes(30).ToString("HH:mm"), result[0].HoraPrevista);
        Assert.Equal("HotelB", result[1].UnidadeHotel);
        Assert.Equal(now.AddMinutes(45).ToString("HH:mm"), result[1].HoraPrevista);
        Assert.Equal("HotelA", result[2].UnidadeHotel);
        Assert.Equal(now.AddHours(2).ToString("HH:mm"), result[2].HoraPrevista);
    }

    [Fact]
    public async Task Tipo2_Ambos_ReturnsUpcomingEntriesAndExits()
    {
        var service = CreateService(out var context);
        var now = DateTime.Now;

        context.Cliente_Movimentos.AddRange(
            new Cliente_Movimento { MovementRID = "ENT-A", Cliente = "Cliente1", Para = "HotelA", De = "Lavandaria", Datetime = now.AddHours(1), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "SAI-A", Cliente = "Cliente1", De = "HotelA", Para = "Lavandaria", Datetime = now.AddMinutes(20), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "ENT-B", Cliente = "Cliente1", Para = "HotelB", De = "Lavandaria", Datetime = now.AddMinutes(50), Quantidade = 1 },
            new Cliente_Movimento { MovementRID = "OTHER", Cliente = "Cliente2", Para = "HotelX", De = "Lavandaria", Datetime = now.AddMinutes(10), Quantidade = 1 });

        await context.SaveChangesAsync();

        var request = new ProximasEntregasRequest(
            Hotel: "Cliente1",
            DataInicio: now.AddDays(-1),
            DataFim: now.AddDays(1),
            Tipo: 2,
            Pagina: 1,
            NumRegistos: 50);

        var result = await service.ProximasEntregasAsync(request);

        Assert.Equal(3, result.Count);
        Assert.Equal("HotelA", result[0].UnidadeHotel);
        Assert.Equal(now.AddMinutes(20).ToString("HH:mm"), result[0].HoraPrevista);
        Assert.Equal("HotelB", result[1].UnidadeHotel);
        Assert.Equal(now.AddMinutes(50).ToString("HH:mm"), result[1].HoraPrevista);
        Assert.Equal("HotelA", result[2].UnidadeHotel);
        Assert.Equal(now.AddHours(1).ToString("HH:mm"), result[2].HoraPrevista);
    }
}
