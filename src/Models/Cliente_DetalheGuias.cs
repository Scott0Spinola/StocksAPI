namespace src.Models;

public class Cliente_DetalheGuias
{
    public Guid Id { get; set; }

     public Guid DocId { get; set; }

    public string NumeroGuia { get; set; } = null!;

    public int NumeroPecas {get; set;}

    public decimal Valor {get; set;}
}