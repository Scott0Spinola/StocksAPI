using Microsoft.EntityFrameworkCore;
using src.Data;
using src.Dtos.Faturacao_Dtos;
using src.Services.TimeSeries;

namespace src.Services.Faturacao;

/// <summary>
/// Business logic for faturação indicators.
/// </summary>
/// <remarks>
/// This service computes totals from <c>Cliente_DetalheFaturas</c> joined to <c>Cliente_Faturas</c>:
/// - <see cref="IndicadoresFaturacaoDTO.TotalFaturacado"/>: gross invoiced amount (includes IVA), net of credit notes.
/// - <see cref="IndicadoresFaturacaoDTO.TotalIVA"/>: IVA amount, net of credit notes.
/// - <see cref="IndicadoresFaturacaoDTO.TotalNotasCredito"/>: gross credit-note amount (includes IVA), always returned as a positive number.
/// - <see cref="IndicadoresFaturacaoDTO.PercDiferencialAnterior"/>: percent change vs previous period window.
/// </remarks>
public class FaturacaoService
{
    private readonly StocksContext _context;
    private readonly ILogger<FaturacaoService> _logger;

    public FaturacaoService(StocksContext context, ILogger<FaturacaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Calculates faturação indicators for the provided period (inclusive), and compares against the previous period.
    /// </summary>
    /// <param name="filtro">Request filter: hotel and date window.</param>
    /// <returns>Computed indicators.</returns>
    public async Task<IndicadoresFaturacaoDTO> IndicadoresAsync(FiltroFaturacao filtro)
    {
        ValidateFiltro(filtro);

        var hotel = filtro.Hotel.Trim();
        var startInclusive = filtro.DataInicio;
        var endInclusive = filtro.DataFim;

        // Reuse the same previous-window approach as the Dashboard time-series indicators:
        // previous window has the same duration and ends immediately before the current window.
        var (previousStartInclusive, previousEndInclusive) = TimeSeriesTabs.GetPreviousWindow(startInclusive, endInclusive);

        var current = await LoadTotalsAsync(hotel, startInclusive, endInclusive);
        var previous = await LoadTotalsAsync(hotel, previousStartInclusive, previousEndInclusive);

        decimal percDiferencialAnterior = 0m;

        // Percent variation: if previous is zero, keep it at 0 to avoid division-by-zero.
        if (previous.TotalFaturacado != 0m)
        {
            percDiferencialAnterior = (current.TotalFaturacado - previous.TotalFaturacado) / previous.TotalFaturacado * 100m;
        }

        _logger.LogInformation(
            "Faturacao indicadores hotel={Hotel} current={Start}-{End} previous={PrevStart}-{PrevEnd} total={Total} iva={Iva} notasCredito={NotasCredito} percDiffPrev={Perc}",
            hotel,
            startInclusive,
            endInclusive,
            previousStartInclusive,
            previousEndInclusive,
            current.TotalFaturacado,
            current.TotalIVA,
            current.TotalNotasCredito,
            percDiferencialAnterior);

        return new IndicadoresFaturacaoDTO(
            TotalFaturacado: current.TotalFaturacado,
            TotalIVA: current.TotalIVA,
            TotalNotasCredito: current.TotalNotasCredito,
            PercDiferencialAnterior: percDiferencialAnterior);
    }

    private static void ValidateFiltro(FiltroFaturacao filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (filtro.DataInicio > filtro.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }
    }

    /// <summary>
    /// Loads aggregate totals for a hotel within a date window (inclusive).
    /// </summary>
    /// <remarks>
    /// Totals are computed from detail rows (<c>Cliente_DetalheFaturas</c>) so that:
    /// - IVA is explicit (<c>Cliente_DetalheFaturas.Iva</c>)
    /// - line amounts can be summed to gross (<c>Valor + Iva</c>)
    ///
    /// Credit-note classification is heuristic because we don't have an explicit boolean/type column:
    /// a document is treated as a credit note if any of these is true:
    /// - header <c>Valor</c> is negative
    /// - <c>NumeroDoc</c> starts with "NC"
    /// - <c>Estado</c> contains "credito"/"crédito" (case-insensitive)
    /// </remarks>
    private async Task<(decimal TotalFaturacado, decimal TotalIVA, decimal TotalNotasCredito)> LoadTotalsAsync(
        string hotel,
        DateTime startInclusive,
        DateTime endInclusive)
    {
        var aggregates = await (
            from d in _context.Cliente_DetalheFaturas.AsNoTracking()
            join f in _context.Cliente_Faturas.AsNoTracking() on d.DocId equals f.Id
            where f.Cliente == hotel
                && f.Data >= startInclusive
                && f.Data <= endInclusive
            let isNotaCredito =
                f.Valor < 0
                || f.NumeroDoc.StartsWith("NC", StringComparison.OrdinalIgnoreCase)
                || (f.Estado != null
                    && (f.Estado.Contains("credito", StringComparison.OrdinalIgnoreCase)
                        || f.Estado.Contains("crédito", StringComparison.OrdinalIgnoreCase)))
            group new { d, isNotaCredito } by 1 into g
            select new
            {
                FaturasGross = g.Sum(x => x.isNotaCredito ? 0m : x.d.Valor + x.d.Iva),
                FaturasIva = g.Sum(x => x.isNotaCredito ? 0m : x.d.Iva),
                NotasCreditoGross = g.Sum(x => x.isNotaCredito ? Math.Abs(x.d.Valor + x.d.Iva) : 0m),
                NotasCreditoIva = g.Sum(x => x.isNotaCredito ? Math.Abs(x.d.Iva) : 0m),
            })
            .SingleOrDefaultAsync();

        if (aggregates == null)
        {
            return (TotalFaturacado: 0m, TotalIVA: 0m, TotalNotasCredito: 0m);
        }

        // Net totals include the effect of credit notes.
        var totalFaturacado = aggregates.FaturasGross - aggregates.NotasCreditoGross;
        var totalIva = aggregates.FaturasIva - aggregates.NotasCreditoIva;
        var totalNotasCredito = aggregates.NotasCreditoGross;

        return (TotalFaturacado: totalFaturacado, TotalIVA: totalIva, TotalNotasCredito: totalNotasCredito);
    }
}
