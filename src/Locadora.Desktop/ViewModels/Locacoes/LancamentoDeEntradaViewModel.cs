using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class LancamentoDeEntradaViewModel(IApiClient api, Action<LocacaoResponse> aoRegistrar) : ViewModelBase
{
    public int LocacaoId { get; set; }
    public IReadOnlyList<FormaDePagamento> Formas => FormasDePagamento.Todas;

    [ObservableProperty]
    private bool _aberto;

    [ObservableProperty]
    private string _valorDigitado = "";

    [ObservableProperty]
    private FormaDePagamento _formaSelecionada = FormasDePagamento.Todas[0];

    [RelayCommand]
    private void Abrir() => Aberto = true;

    [RelayCommand]
    private void Fechar()
    {
        Aberto = false;
        MensagemErro = null;
        ValorDigitado = "";
    }

    [RelayCommand]
    public async Task RegistrarEntradaAsync()
    {
        if (!ValorEmReais.TentarLer(ValorDigitado, out var valor))
        {
            MensagemErro = "Informe o valor da entrada, por exemplo 100,50.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            var pagamento = new PagamentoRequest(valor, FormaSelecionada.Valor);
            aoRegistrar(await api.RegistrarPagamentoAsync(LocacaoId, pagamento));
            Fechar();
        });
    }
}
