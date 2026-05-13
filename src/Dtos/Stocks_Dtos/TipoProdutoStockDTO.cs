using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Stocks_Dtos;

public record TipoProdutoStockDTO
(
    [Required]
    string Produto,

    [Required]
    int Qtd,

    [Required]
    int QtdMais60dias,

    [Required]
    int QtdMenos60dias
);
