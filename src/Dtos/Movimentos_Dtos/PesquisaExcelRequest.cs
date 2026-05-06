using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Movimentos_Dtos;

/// <summary>
/// Request payload for exporting the Pesquisa series to Excel.
/// </summary>
/// <remarks>
/// This intentionally omits pagination fields (<c>pagina</c>/<c>numRegistos</c>).
///
/// Reason:
/// - The JSON endpoint is optimized for UI paging.
/// - An Excel export is typically expected to contain the full series for the requested window.
///
/// The service method <c>PesquisaExcelAsync</c> always exports the full series.
/// </remarks>
public record PesquisaExcelRequest
(
    [Required]
    [MaxLength(80)]
    string Hotel,

    [Required]
    [Range(0, 4)]
    int Tab,

    [Required]
    DateTime DataInicio,

    [Required]
    DateTime DataFim,

    [Required]
    [Range(0, 2)]
    int Tipo
);
