namespace src.Models;

public class Cliente_Faturas
{
    public string? Cliente {get; set;}

    public string? NumeroDoc {get; set;}

    public DateTime Data {get; set;}

    public decimal Valor {get; set;}

    public string? Estado {get; set;}

    public string? UrlDocument {get; set;}
}
