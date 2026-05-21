namespace src.Models; 

public class Cliente_DetalheFaturas
{
    public string? Cliente {get; set;}

    public string? NumeroDoc {get; set;}

    public string? Servico {get; set;}

    public string? Produto {get; set;}

    public string? Codigo {get; set;}

    public int Quantidade {get; set;}

    public decimal Valor {get; set;}

    public decimal Iva {get; set;}
}