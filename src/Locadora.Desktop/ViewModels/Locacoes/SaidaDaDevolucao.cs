using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Api.Dtos.Locacoes;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class SaidaDaDevolucao(Action aoAlterarValor) : ObservableObject
{
    public IReadOnlyList<FormaDePagamento> Formas => FormasDePagamento.Todas;

    [ObservableProperty]
    private string _valorDigitado = "";

    [ObservableProperty]
    private FormaDePagamento _formaSelecionada = FormasDePagamento.Todas[0];

    public decimal? Valor => ValorMonetario.TentarLer(ValorDigitado, out var valor) ? valor : null;

    public PagamentoRequest ParaRequisicao(decimal valor) => new(valor, FormaSelecionada.Valor);

    partial void OnValorDigitadoChanged(string value) => aoAlterarValor();
}
