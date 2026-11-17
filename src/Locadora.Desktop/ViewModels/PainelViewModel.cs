using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Painel;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels.Locacoes;
using Locadora.Desktop.ViewModels.Painel;

namespace Locadora.Desktop.ViewModels;

public partial class PainelViewModel(IApiClient apiClient, INavegacao navegacao) : ViewModelBase
{
    private static readonly string[] StatusDaFrotaEmOrdem = ["Disponivel", "Locado", "Manutencao"];

    public string Titulo => "Painel";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SemAtrasadas))]
    private IReadOnlyList<LocacaoAtrasadaResponse> _atrasadas = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SemVencimentosHoje))]
    private IReadOnlyList<LocacaoVencendoResponse> _vencendoHoje = [];

    [ObservableProperty]
    private IReadOnlyList<ContadorDaFrota> _frota = [];

    [ObservableProperty]
    private bool _carregado;

    public bool SemAtrasadas => Atrasadas.Count == 0;
    public bool SemVencimentosHoje => VencendoHoje.Count == 0;

    [RelayCommand]
    private Task CarregarPainelAsync() => ExecutarAsync(async () =>
    {
        var painel = await apiClient.ObterPainelAsync();
        Atrasadas = painel.Atrasadas;
        VencendoHoje = painel.VencendoHoje;
        Frota = MontarContadoresDaFrota(painel.Frota);
        Carregado = true;
    });

    [RelayCommand]
    private void RegistrarRetirada() => navegacao.NavegarPara<NovaLocacaoViewModel>();

    [RelayCommand]
    private void AbrirLocacao(int locacaoId) =>
        navegacao.NavegarPara<LocacaoDetalheViewModel>(contrato => contrato.Carregar(locacaoId));

    private static List<ContadorDaFrota> MontarContadoresDaFrota(IReadOnlyCollection<ContadorFrotaResponse> contadores)
    {
        var total = contadores.Sum(contador => contador.Quantidade);
        return StatusDaFrotaEmOrdem.Select(status => CriarContador(status, contadores, total)).ToList();
    }

    private static ContadorDaFrota CriarContador(string status, IReadOnlyCollection<ContadorFrotaResponse> contadores, int total)
    {
        var quantidade = contadores.FirstOrDefault(contador => contador.Status == status)?.Quantidade ?? 0;
        var participacao = total == 0 ? 0 : (double)quantidade / total;
        return new ContadorDaFrota(status, quantidade, participacao);
    }
}
