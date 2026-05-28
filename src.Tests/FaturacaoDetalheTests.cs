using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using src.Data;
using src.Dtos.Faturacao_Dtos;
using src.Models;
using src.Services.Faturacao;
using Xunit;

namespace src.Tests;

public class FaturacaoDetalheTests
{
    private static FaturacaoService CreateService(out StocksContext context)
    {
        var options = new DbContextOptionsBuilder<StocksContext>()
            .UseInMemoryDatabase(databaseName: $"{nameof(FaturacaoDetalheTests)}_{Guid.NewGuid()}")
            .Options;

        context = new StocksContext(options);
        context.Database.EnsureCreated();
        return new FaturacaoService(context, NullLogger<FaturacaoService>.Instance);
    }

    [Fact]
    public async Task DetalheAsync_GroupsByProdutoServico_AndComputesPercentVsPreviousWindow()
    {
        var service = CreateService(out var context);

        var startInclusive = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Unspecified);
        var endInclusive = startInclusive.AddDays(1).AddTicks(-1);

        var invoiceCurrentId = Guid.NewGuid();
        var creditNoteCurrentId = Guid.NewGuid();
        var invoicePreviousId = Guid.NewGuid();

        context.Cliente_Faturas.AddRange(
            new Cliente_Faturas
            {
                Id = invoiceCurrentId,
                Cliente = "HotelA",
                NumeroDoc = "FT 1",
                Data = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Unspecified),
                Valor = 100m,
                Estado = "Fechado"
            },
            new Cliente_Faturas
            {
                Id = creditNoteCurrentId,
                Cliente = "HotelA",
                NumeroDoc = "NC 1",
                Data = new DateTime(2026, 5, 10, 14, 0, 0, DateTimeKind.Unspecified),
                Valor = -20m,
                Estado = "Crédito"
            },
            new Cliente_Faturas
            {
                Id = invoicePreviousId,
                Cliente = "HotelA",
                NumeroDoc = "FT 0",
                Data = new DateTime(2026, 5, 9, 12, 0, 0, DateTimeKind.Unspecified),
                Valor = 50m,
                Estado = "Fechado"
            },
            // Noise: other hotel
            new Cliente_Faturas
            {
                Id = Guid.NewGuid(),
                Cliente = "HotelB",
                NumeroDoc = "FT X",
                Data = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Unspecified),
                Valor = 999m,
                Estado = "Fechado"
            });

        context.Cliente_DetalheFaturas.AddRange(
            new Cliente_DetalheFaturas
            {
                DocId = invoiceCurrentId,
                NumeroDoc = "FT 1",
                Produto = "P1",
                Servico = "S1",
                Quantidade = 2,
                Valor = 80m,
                Iva = 20m
            },
            new Cliente_DetalheFaturas
            {
                DocId = creditNoteCurrentId,
                NumeroDoc = "NC 1",
                Produto = "P1",
                Servico = "S1",
                Quantidade = 1,
                Valor = 16m,
                Iva = 4m
            },
            new Cliente_DetalheFaturas
            {
                DocId = invoicePreviousId,
                NumeroDoc = "FT 0",
                Produto = "P1",
                Servico = "S1",
                Quantidade = 1,
                Valor = 40m,
                Iva = 10m
            },
            // Current-only second group
            new Cliente_DetalheFaturas
            {
                DocId = invoiceCurrentId,
                NumeroDoc = "FT 1",
                Produto = "P2",
                Servico = "S2",
                Quantidade = 3,
                Valor = 30m,
                Iva = 0m
            });

        await context.SaveChangesAsync();

        var result = await service.DetalheAsync(new FiltroFaturacao(Hotel: "HotelA", DataInicio: startInclusive, DataFim: endInclusive));

        var p1s1 = Assert.Single(result, x => x.Produto == "P1" && x.Servico == "S1");
        Assert.Equal(1, p1s1.Qtd);
        Assert.Equal(80m, p1s1.Valor);
        Assert.Equal(60m, p1s1.PercDiferencialAnterior);

        var p2s2 = Assert.Single(result, x => x.Produto == "P2" && x.Servico == "S2");
        Assert.Equal(3, p2s2.Qtd);
        Assert.Equal(30m, p2s2.Valor);
        Assert.Equal(0m, p2s2.PercDiferencialAnterior);
    }
}
