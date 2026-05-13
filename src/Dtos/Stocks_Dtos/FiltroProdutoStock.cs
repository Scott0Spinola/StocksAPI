using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Stocks_Dtos;

public record FiltroProdutoStock
(
    [Required]
    [MaxLength(80)]
    string Hotel,

    [Required]
    [MaxLength(200)]
    string TipoProduto
);
