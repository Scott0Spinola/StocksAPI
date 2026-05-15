namespace IntervencoesAPI.Dtos.ClienteDtos;

public record GetCliente
(
    int Id,

    int IdEntidade,

	string Nome,

	string Referencia,

	string Observacoes,

	int Estado,

	string NProcesso,

	DateTime DataDeInicio,

    DateTime DataActualizacao,

	int CliCampo1,

	int CliCampo2,

	string CliCampo3,

	string CliCampo4
);