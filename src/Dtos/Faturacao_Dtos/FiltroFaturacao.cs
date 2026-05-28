using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Faturacao_Dtos;

public record FiltroFaturacao(
    [Required]
    [MaxLength(80)]
    string Hotel,

    [Required]
    DateTime DataInicio,

    [Required]
    DateTime DataFim,

    [Range(0, 4)]
    int Tab = 4
);
