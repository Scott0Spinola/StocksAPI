using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Faturacao_Dtos;

public record DocumentoFaturaDetalheDTO(
    [Required]
    [MaxLength(255)]
    string Produto,

    [Required]
    [MaxLength(255)]
    string Servico,

    [Range(int.MinValue, int.MaxValue)]
    int Qtd,

    decimal Valor,

    decimal PercDiferencialAnterior
);
