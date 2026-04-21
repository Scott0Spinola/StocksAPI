using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Dashboard_Dtos;

public record UltimasDescargasItem
(
    [Required]
    [MaxLength(10)]
    string Data,

    [MaxLength(10)]
    string Hora,

    [Required]
    [Range(0, 1)]
    int Direcao,

    [Required]
    int Qtd
);
