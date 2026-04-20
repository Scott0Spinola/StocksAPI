namespace src.Dtos.Dashboard_Dtos;

public record IndicadorDirecao
(
    int NumPecas,
    int NumPecasAnterior,
    decimal Peso
);

public record EntradasSaidasIndicadorResponse
(
    IndicadorDirecao Saidas,
    IndicadorDirecao Entradas,
    IndicadorDirecao Diferenca
);
