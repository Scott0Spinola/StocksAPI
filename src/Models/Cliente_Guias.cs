namespace src.Models;

public class Cliente_Guias
{
    public Guid Id { get; set; }

    public string? Cliente {get; set;}

    public string NumeroGuia { get; set; } = null!;

    public DateTime Data {get; set;}

    public int TotalPecas {get; set;}

    public string? UrlDocumento {get; set;}
}