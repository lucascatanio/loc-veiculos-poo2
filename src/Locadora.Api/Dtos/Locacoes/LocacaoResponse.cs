namespace Locadora.Api.Dtos.Locacoes;

public record LocacaoResponse(
    int Id, int ClienteId, string ClienteNome, int VeiculoId, string VeiculoPlaca, string VeiculoModelo,
    DateTime DataRetirada, DateTime DataPrevistaDevolucao, int KmRetirada, string? ObservacaoRetirada,
    decimal ValorDiaria, decimal ValorPrevisto, decimal ValorMulta, decimal ValorTotal, decimal ValorPago,
    decimal SaldoDevedor, string Status, List<PagamentoResponse> Pagamentos);
