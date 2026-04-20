using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Dashboard_Dtos;

public record EntradasSaidasIndicadorRequest
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
    DateTime DataFim
);
