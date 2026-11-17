namespace Locadora.Desktop.ViewModels.Locacoes;

// Valor é o nome do enum que a API espera; Nome é o que aparece na tela.
public sealed record FormaDePagamento(string Valor, string Nome);

public static class FormasDePagamento
{
    public static IReadOnlyList<FormaDePagamento> Todas { get; } =
    [
        new("Dinheiro", "Dinheiro"),
        new("CartaoCredito", "Cartão de crédito"),
        new("CartaoDebito", "Cartão de débito"),
        new("Pix", "Pix")
    ];

    public static string NomeDe(string valor) =>
        Todas.FirstOrDefault(forma => forma.Valor == valor)?.Nome ?? valor;
}
