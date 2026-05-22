using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Documentos_Dtos;

public record FiltroGuia
(
    [Required]
    [MaxLength(80)]
    string Hotel,

    [MaxLength(255)]
    string? NumDocumento,

    DateTime? DataInicio,

    DateTime? DataFim,

    [Range(1, int.MaxValue)]
    int Pagina = 1,

    [Range(1, 500)]
    int NumRegistos = 12
);
