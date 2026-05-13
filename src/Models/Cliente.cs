using System;

namespace src.Models;

public class Cliente
{
    public int Id { get; set; }

    public int IdEntidade { get; set; }

    public string Referencia { get; set; } = string.Empty;

    public string Observacoes { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public int Estado { get; set; }

    public string NProcesso { get; set; } = string.Empty;

    public DateTime DataDeInicio { get; set; }

    public DateTime DataActualizacao { get; set; }

    public int CliCampo1 { get; set; }

    public int CliCampo2 { get; set; }

    public string CliCampo3 { get; set; } = string.Empty;

    public string CliCampo4 { get; set; } = string.Empty;


    public Entidade? Entidade { get; set; }
}
