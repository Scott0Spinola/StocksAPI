using System;

namespace src.Models;

public class Entidade
{
    public int Id { get; set; }

    public string Referencia { get; set; } = string.Empty;

    public string? NomeSocial { get; set; }

    public string? Contribuinte { get; set; }

    public string? Observacoes { get; set; }

    public byte Tipo { get; set; }

    public short? Estado { get; set; }

    public bool Item1 { get; set; }

    public int Item2 { get; set; }

    public string? Item3 { get; set; }

    public string? DesignacaoComercial { get; set; }

    public DateTime DataActualizacao { get; set; }

    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
