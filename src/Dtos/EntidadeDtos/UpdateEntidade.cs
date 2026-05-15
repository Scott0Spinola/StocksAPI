using System.ComponentModel.DataAnnotations;

namespace IntervencoesAPI.Dtos.EntidadeDtos;

public record  UpdateEntidade
(
    [Required]
    string Referencia,


    string? NomeSocial,


    string? Contribuinte,


    string? Observacoes,


    [Required]
    byte Tipo,


    short? Estado,


    [Required]
    bool Item1,


    [Required]
    int Item2,


    string? Item3,


    string? DesignacaoComercial
);
