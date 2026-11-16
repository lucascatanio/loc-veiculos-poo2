namespace Locadora.Api.Dtos.Locacoes;

public record PreviaDevolucaoResponse(
    int DiariasContratadas, int DiasAtraso, decimal ValorPrevisto, decimal ValorMulta,
    decimal ValorTotal, decimal ValorPago, decimal Saldo);
