namespace Locadora.Api.Dtos.Painel;

public record LocacaoVencendoResponse(
    int LocacaoId,
    string ClienteNome,
    string ClienteTelefone,
    string VeiculoPlaca,
    string VeiculoModelo,
    DateTime DataPrevistaDevolucao);
