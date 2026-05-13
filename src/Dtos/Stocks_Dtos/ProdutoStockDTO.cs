using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Stocks_Dtos;

public record ProdutoStockDTO
(
    [Required]
    string Rfid,

    [Required]
    DateTime DataMovimento
);
