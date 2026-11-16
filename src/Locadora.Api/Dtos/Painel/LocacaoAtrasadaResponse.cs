namespace Locadora.Api.Dtos.Painel;

public record LocacaoAtrasadaResponse(
    int LocacaoId,
    string ClienteNome,
    string ClienteTelefone,
    string VeiculoPlaca,
    string VeiculoModelo,
    DateTime DataPrevistaDevolucao,
    int MinutosAtraso,
    int DiasAtraso,
    decimal MultaEstimada);
