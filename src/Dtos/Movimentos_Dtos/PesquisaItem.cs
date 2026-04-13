using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Movimentos_Dtos;

public record PesquisaItem
(
    [MaxLength(80)]
    string? NumDocumento,

    [Required]
    DateTime Data,

    [Required]
    [Range(0, 1)]
    int Direcao,

    [Required]
    [MaxLength(50)]
    string Tipo,

    [MaxLength(100)]
    string? Produto,

    [Required]
    int Qtd,

    [Required]
    int IdDoc
);
