namespace Locadora.Api.Dtos.Locacoes;

public record LocacaoResumoResponse(
    int Id, string ClienteNome, string VeiculoPlaca, string VeiculoModelo,
    DateTime DataRetirada, DateTime DataPrevistaDevolucao,
    decimal ValorTotal, decimal ValorPago, decimal SaldoDevedor, string Status);
