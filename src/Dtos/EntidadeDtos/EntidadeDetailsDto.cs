namespace IntervencoesAPI.Dtos.EntidadeDtos;

public record IntervencaoDto(
    int Id,
    int ProcessoId,
    string? Autor,
    int Tipo,
    int Estado,
    string Tema
    );


public record ProcessoProjectoDto(
    int Id,
    string Referencia,
    int Estado,
    DateTime DataInicio,
    List<IntervencaoDto> IntervencaoDtos 

);

public record ClienteDto(
    int Id,
    int IdEntidade,
    string Referencia,
    string Observacoes,
    int Estado,
    string NProcesso,
    List<ProcessoProjectoDto> ProcessoProjectos
);


public record EntidadeDetailsDto(
    int Id,
    string Referencia,
    string? NomeSocial,
    string? Contribuinte,
    string? Observacoes,
    byte Tipo,
    short? Estado,
    bool Item1,
    int Item2,
    string? Item3,
    string? DesignacaoComercial,
    DateTime DataActualizacao,
    List<ClienteDto> Clientes
);