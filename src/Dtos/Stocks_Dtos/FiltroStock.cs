using System.ComponentModel.DataAnnotations;

namespace src.Dtos.Stocks_Dtos;

public record FiltroStock
(
    [Required]
    [MaxLength(80)]
    string Hotel
);
