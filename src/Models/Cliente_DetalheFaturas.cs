namespace src.Models; 

public class Cliente_DetalheFaturas
{
    public Guid Id { get; set; }

     public Guid DocId { get; set; }

    public string? Cliente {get; set;}

    public string NumeroDoc { get; set; } = null!;

    public string? Servico {get; set;}

    public string? Produto {get; set;}

    public string? Codigo {get; set;}

    public int Quantidade {get; set;}

    public decimal Valor {get; set;}

    public decimal Iva {get; set;}
}