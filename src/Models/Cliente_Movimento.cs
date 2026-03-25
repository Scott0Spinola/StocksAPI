using System;

namespace src.Models;

public class Cliente_Movimento
{
    public int Id { get; set; }
    public  string? MovementRID {get; set;}

    public string? De {get; set;}

    public string? Para {get; set;}

    public string? Cliente {get; set;}

    public string? Descricao {get; set;}

    public DateTime Datetime {get; set;} 
    
    public string? DataFormatada {get; set;}

    public int Quantidade {get; set;}
}
