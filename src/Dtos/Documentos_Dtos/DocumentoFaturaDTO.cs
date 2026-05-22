using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace src.Dtos.Documentos_Dtos;

public record DocumentoFaturaDTO
(
    [Required]
    [MaxLength(255)]
    string NumDocumento,

    [Required]
    [MaxLength(7)]
    string MesAno,

    [Required]
    decimal Valor,

    [property: JsonPropertyName("Estado")]
    string? Estado,

    [Required]
    Guid IdDoc,

    [Range(1, int.MaxValue)]
    int Pagina,

    [Range(1, 500)]
    int NumRegistos
);
