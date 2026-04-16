using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Movimentos_Dtos;

public record EvolucaoRequest
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
    int Tipo,

    [Required]
    [Range(1, int.MaxValue)]
    int Pagina,

    [Required]
    [Range(1, 500)]
    int NumRegistos
);
