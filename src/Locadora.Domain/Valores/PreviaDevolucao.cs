namespace Locadora.Domain.Valores;

public record PreviaDevolucao(
    int DiariasContratadas, int DiasAtraso, decimal ValorPrevisto, decimal ValorMulta,
    decimal ValorTotal, decimal ValorPago, decimal Saldo);
