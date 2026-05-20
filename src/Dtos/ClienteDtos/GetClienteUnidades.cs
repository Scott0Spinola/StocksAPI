namespace IntervencoesAPI.Dtos.ClienteDtos;

public record GetClienteUnidades(
    string Cliente,
    IReadOnlyList<string> Unidades
);
