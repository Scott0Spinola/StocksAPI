using System;

namespace src.Models;

public class Cliente_Tag
{
    public int Id { get; set; }
    public string? Unidade {get; set;}

    public string? Localizacao {get; set;}

    public string? Produto {get; set;}

    public string? EPC {get; set;}

    public string? Estado {get; set;}

    public DateTime Data_Ultimo_Movimento {get; set;}

    public string? Ultima_Localizacao {get; set;}

    public string? Ultima_Unidade {get; set;}

    public Cliente? Nome { get; set; }
}
