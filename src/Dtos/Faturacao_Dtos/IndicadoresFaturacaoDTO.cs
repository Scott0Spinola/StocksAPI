using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Faturacao_Dtos;

public record IndicadoresFaturacaoDTO(
    [Required]
    decimal TotalFaturacado,

    [Required]
    decimal TotalIVA,

    [Required]
    decimal TotalNotasCredito,

    [Required]
    decimal PercDiferencialAnterior
);
