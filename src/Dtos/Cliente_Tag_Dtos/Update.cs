using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Cliente_Tag_Dtos;

public record Update
(
    [Required]
    [MaxLength (50)]
    string Unidade,

    [Required]
    [MaxLength (50)]
    string Localizacao,

    [Required]
    [MaxLength (50)]
    string Produto,

    [Required]
    [MaxLength (80)]
    string EPC,

    [Required]
    [MaxLength (5)]
    string Estado,

    [MaxLength (50)]
    string Ultima_Localizacao,

    [MaxLength (50)]
    string Ultima_Unidade
);
