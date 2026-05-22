using System;
using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Documentos_Dtos;

public record DocumentoGuiaDTO
(
    [Required]
    [MaxLength(255)]
    string NumDocumento,

    [Required]
    [MaxLength(10)]
    string Data,

    [Required]
    int TotalPecas,

    [Required]
    Guid IdDoc,

    [Range(1, int.MaxValue)]
    int Pagina,

    [Range(1, 500)]
    int NumRegistos
);
