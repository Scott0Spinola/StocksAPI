using System.ComponentModel.DataAnnotations;

namespace IntervencoesAPI.Dtos;

public record CreateCliente(
	[Required]
    int IdEntidade,

    [Required]
    [MaxLength(500)]
	string Referencia,

    [Required]
    [MaxLength(255)]
	string Observacoes,

    [Required]
	int Estado,

    [Required]
    [MaxLength(10)]
	string NProcesso,

    [Required]
	int CliCampo1,

    [Required]
	int CliCampo2,

    [Required]
    [MaxLength(100)]
	string CliCampo3,

    [Required]
    [MaxLength(100)]
	string CliCampo4
    ,

    [MaxLength(255)]
    string Nome = ""
);
