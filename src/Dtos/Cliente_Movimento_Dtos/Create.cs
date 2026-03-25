using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Cliente_Movimento_Dtos;


public record Create
(
    [Required]
    [MaxLength (80)]
    string MovementRID,

    [Required]
    [MaxLength (50)]
    string De,

    [Required]
    [MaxLength (50)]
    string Para,

    [Required]
    [MaxLength (50)]
    string Cliente,

    [Required]
    [MaxLength (100)]
    string Descricao,

    [Required]
    [MaxLength (10)]
    string DataFormatada,

    [Required]
    int Quantidade
);
