using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class LocacoesViewModel : ViewModelBase
{
    private readonly IApiClient _api;
    private readonly INavegacao _navegacao;

    public LocacoesViewModel(IApiClient api, INavegacao navegacao)
    {
        _api = api;
        _navegacao = navegacao;
        _statusSelecionado = OpcaoDeStatus.Abertas;
    }

    public string Titulo => "Locações";
    public IReadOnlyList<OpcaoDeStatus> Opcoes => OpcaoDeStatus.Todas;

    [ObservableProperty]
    private OpcaoDeStatus _statusSelecionado;

    [ObservableProperty]
    private IReadOnlyList<LocacaoLinha> _locacoes = [];

    [ObservableProperty]
    private bool _carregou;

    [ObservableProperty]
    private LocacaoLinha? _locacaoSelecionada;

    public bool ListaVazia => Carregou && Locacoes.Count == 0 && MensagemErro is null;

    [RelayCommand]
    public async Task CarregarLocacoesAsync()
    {
        await ExecutarAsync(async () =>
        {
            var resumos = await _api.ListarLocacoesAsync(StatusSelecionado.Valor);
            var agora = DateTime.Now;
            Locacoes = resumos.Select(resumo => new LocacaoLinha(resumo, agora)).ToList();
        });
        Carregou = true;
        OnPropertyChanged(nameof(ListaVazia));
    }

    [RelayCommand]
    private void NovaLocacao() => _navegacao.NavegarPara<NovaLocacaoViewModel>();

    partial void OnStatusSelecionadoChanged(OpcaoDeStatus value) => _ = CarregarLocacoesAsync();

    partial void OnLocacaoSelecionadaChanged(LocacaoLinha? value)
    {
        if (value is null) return;
        _navegacao.NavegarPara<LocacaoDetalheViewModel>(detalhe => detalhe.Carregar(value.Id));
    }
}
