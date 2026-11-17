namespace Locadora.Desktop.ViewModels.Locacoes;

public sealed record OpcaoDeCategoria(int? Id, string Nome)
{
    public static OpcaoDeCategoria Todas { get; } = new(null, "Todas");
}
