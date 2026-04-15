using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Movimentos_Dtos;

public record PesquisaRequest
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

    [MaxLength(80)]
    string? NumGuia,

    [Required]
    [Range(0, 2)]
    int Tipo
);
