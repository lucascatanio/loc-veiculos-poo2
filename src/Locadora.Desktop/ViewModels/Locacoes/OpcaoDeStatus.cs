namespace Locadora.Desktop.ViewModels.Locacoes;

public sealed record OpcaoDeStatus(string Valor, string Titulo, string TextoVazio)
{
    public static readonly OpcaoDeStatus Abertas = new("Aberta", "Abertas", "Nenhuma locação aberta.");
    public static readonly OpcaoDeStatus Finalizadas = new("Finalizada", "Finalizadas", "Nenhuma locação finalizada.");
    public static readonly OpcaoDeStatus Canceladas = new("Cancelada", "Canceladas", "Nenhuma locação cancelada.");

    public static IReadOnlyList<OpcaoDeStatus> Todas { get; } = [Abertas, Finalizadas, Canceladas];
}
