using System.Globalization;
using Expedita.Export.Excel;
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
        var (startInclusive, endInclusive) = TimeSeriesTabs.GetWindow(filtro.DataInicio, filtro.DataFim, filtro.Tab);

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

    /// <summary>
    /// Lists faturação totals grouped by product and service for the provided period (inclusive),
    /// including percent change vs the previous period window.
    /// </summary>
    public async Task<List<DocumentoFaturaDetalheDTO>> DetalheAsync(FiltroFaturacao filtro)
    {
        ValidateFiltro(filtro);

        var hotel = filtro.Hotel.Trim();
        var (startInclusive, endInclusive) = TimeSeriesTabs.GetWindow(filtro.DataInicio, filtro.DataFim, filtro.Tab);

        var (previousStartInclusive, previousEndInclusive) = TimeSeriesTabs.GetPreviousWindow(startInclusive, endInclusive);

        var current = await LoadDetalheAsync(hotel, startInclusive, endInclusive);
        var previous = await LoadDetalheAsync(hotel, previousStartInclusive, previousEndInclusive);

        var previousByKey = previous.ToDictionary(
            x => (x.Produto, x.Servico),
            x => x.Valor);

        var result = current
            .Select(x =>
            {
                previousByKey.TryGetValue((x.Produto, x.Servico), out var prevValor);

                decimal perc = 0m;
                if (prevValor != 0m)
                {
                    perc = (x.Valor - prevValor) / prevValor * 100m;
                }

                return new DocumentoFaturaDetalheDTO(
                    Produto: x.Produto,
                    Servico: x.Servico,
                    Qtd: x.Qtd,
                    Valor: x.Valor,
                    PercDiferencialAnterior: perc);
            })
            .OrderByDescending(x => x.Valor)
            .ThenBy(x => x.Produto)
            .ThenBy(x => x.Servico)
            .ToList();

        _logger.LogInformation(
            "Faturacao detalhe hotel={Hotel} current={Start}-{End} previous={PrevStart}-{PrevEnd} result={Count}",
            hotel,
            startInclusive,
            endInclusive,
            previousStartInclusive,
            previousEndInclusive,
            result.Count);

        return result;
    }

    /// <summary>
    /// Generates an Excel (.xlsx) export for the faturação detalhe listing.
    /// </summary>
    public async Task<(byte[] Content, string FileName)> ExportacaoAsync(FiltroFaturacao filtro)
    {
        // Reuse the same core logic as the JSON endpoint.
        var items = await DetalheAsync(filtro);

        // Allocate one spare row/column to avoid any edge-indexing behavior in the library.
        var numRows = Math.Max(2, items.Count + 2); // header + data + spare
        const int numCols = 6; // 5 data columns + spare

        var workDir = Path.Combine(Path.GetTempPath(), "StocksAPI", "exports");
        Directory.CreateDirectory(workDir);

        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_faturacao_detalhe.xlsx";
        var fullPath = Path.Combine(workDir, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        Stream? fileStream = null;

        try
        {
            var doc = new xlsxDocumento(workDir);
            var page = doc.AdicionarPagina("FaturacaoDetalhe", numRows, numCols);

            // Header row
            SetCell(page, col0: 0, row0: 0, "Produto", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 1, row0: 0, "Servico", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 2, row0: 0, "Qtd", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 3, row0: 0, "Valor", xlsxCelula.tiposValor.TextoHeader);
            SetCell(page, col0: 4, row0: 0, "PercDiferencialAnterior", xlsxCelula.tiposValor.TextoHeader);

            // Data rows
            for (var i = 0; i < items.Count; i++)
            {
                var row0 = i + 1;
                var item = items[i];

                SetCell(page, col0: 0, row0, item.Produto ?? string.Empty, xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 1, row0, item.Servico ?? string.Empty, xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 2, row0, item.Qtd.ToString(CultureInfo.InvariantCulture), xlsxCelula.tiposValor.Inteiro);
                SetCell(page, col0: 3, row0, item.Valor.ToString("0.00", CultureInfo.InvariantCulture), xlsxCelula.tiposValor.Texto);
                SetCell(page, col0: 4, row0, item.PercDiferencialAnterior.ToString("0.00", CultureInfo.InvariantCulture), xlsxCelula.tiposValor.Texto);
            }

            doc.Exportar(fileName, xlsxDocumento.tipoExportacao.documentoXls, workDir, ref fileStream);
            fileStream?.Flush();
        }
        finally
        {
            fileStream?.Dispose();
        }

        var content = await File.ReadAllBytesAsync(fullPath);

        try
        {
            File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete temporary Excel export file: {Path}", fullPath);
        }

        return (content, fileName);
    }

    private static void ValidateFiltro(FiltroFaturacao filtro)
    {
        if (string.IsNullOrWhiteSpace(filtro.Hotel))
        {
            throw new ArgumentException("'hotel' is required.");
        }

        if (filtro.Tab is < 0 or > 4)
        {
            throw new ArgumentException("'tab' must be 0 (dia), 1 (semana), 2 (mes), 3 (ano), or 4 (personalizado). ");
        }

        if (filtro.DataInicio > filtro.DataFim)
        {
            throw new ArgumentException("'dataInicio' must be less than or equal to 'dataFim'.");
        }
    }

    /// <summary>
    /// Writes a single cell into an <see cref="xlsxPagina"/>.
    /// </summary>
    private static void SetCell(xlsxPagina page, int col0, int row0, string? value, xlsxCelula.tiposValor tipo)
    {
        var safeValue = value ?? string.Empty;

        var cell = new xlsxCelula(col0, row0)
        {
            idxColuna = col0,
            idxLinha = row0,
            valor = safeValue,
            tipoValor = tipo
        };

        // NOTE: this is row/col order.
        page.set_Celula(row0, col0, cell);
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
                || f.NumeroDoc.StartsWith("NC")
                || (f.Estado != null
                    && (f.Estado.Contains("credito")
                        || f.Estado.Contains("crédito")))
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

    private async Task<List<(string Produto, string Servico, int Qtd, decimal Valor)>> LoadDetalheAsync(
        string hotel,
        DateTime startInclusive,
        DateTime endInclusive)
    {
        var query =
            from d in _context.Cliente_DetalheFaturas.AsNoTracking()
            join f in _context.Cliente_Faturas.AsNoTracking() on d.DocId equals f.Id
            where f.Cliente == hotel
                && f.Data >= startInclusive
                && f.Data <= endInclusive
            let isNotaCredito =
                f.Valor < 0
                || f.NumeroDoc.StartsWith("NC")
                || (f.Estado != null
                    && (f.Estado.Contains("credito")
                        || f.Estado.Contains("crédito")))
            select new
            {
                Produto = d.Produto ?? string.Empty,
                Servico = d.Servico ?? string.Empty,
                QtdSigned = isNotaCredito ? -Math.Abs(d.Quantidade) : d.Quantidade,
                ValorSigned = isNotaCredito ? -Math.Abs(d.Valor + d.Iva) : (d.Valor + d.Iva)
            };

        var aggregates = await query
            .GroupBy(x => new { x.Produto, x.Servico })
            .Select(g => new
            {
                g.Key.Produto,
                g.Key.Servico,
                Qtd = g.Sum(x => x.QtdSigned),
                Valor = g.Sum(x => x.ValorSigned),
            })
            .ToListAsync();

        return aggregates
            .Select(x => (x.Produto, x.Servico, x.Qtd, x.Valor))
            .ToList();
    }
}
