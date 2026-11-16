namespace Locadora.Api.Dtos.Painel;

public record PainelResponse(
    List<LocacaoAtrasadaResponse> Atrasadas,
    List<LocacaoVencendoResponse> VencendoHoje,
    List<ContadorFrotaResponse> Frota);
