using CommunityToolkit.Mvvm.ComponentModel;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class EntradaDaRetirada(Action aoMudar) : ObservableObject
{
    public IReadOnlyList<FormaDePagamento> Formas => FormasDePagamento.Todas;

    [ObservableProperty]
    private string _valorDigitado = "";

    [ObservableProperty]
    private FormaDePagamento _formaSelecionada = FormasDePagamento.Todas[0];

    public bool Preenchida => !string.IsNullOrWhiteSpace(ValorDigitado);

    public decimal? Valor => ValorMonetario.TentarLer(ValorDigitado, out var valor) ? valor : null;

    partial void OnValorDigitadoChanged(string value) => aoMudar();
}
