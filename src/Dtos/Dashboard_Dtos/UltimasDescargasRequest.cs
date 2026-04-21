using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Dashboard_Dtos;

public record UltimasDescargasRequest
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
