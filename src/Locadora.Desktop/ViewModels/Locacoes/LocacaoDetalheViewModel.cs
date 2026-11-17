using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class LocacaoDetalheViewModel : ViewModelBase
{
    private const string FormatoDeData = "dd/MM/yyyy HH:mm";
    private const string MensagemDeCancelamento =
        "O veículo volta a ficar disponível. As entradas já pagas continuam registradas; o estorno é feito fora do sistema.";
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IApiClient _api;
    private readonly INavegacao _navegacao;
    private readonly IDialogos _dialogos;

    public LocacaoDetalheViewModel(IApiClient api, INavegacao navegacao, IDialogos dialogos)
    {
        _api = api;
        _navegacao = navegacao;
        _dialogos = dialogos;
        Entrada = new LancamentoDeEntradaViewModel(api, Aplicar);
        Prorrogacao = new ProrrogacaoViewModel(api, Aplicar);
    }

    public LancamentoDeEntradaViewModel Entrada { get; }
    public ProrrogacaoViewModel Prorrogacao { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Carregada), nameof(EstaAberta))]
    private LocacaoResponse? _locacao;

    [ObservableProperty]
    private IReadOnlyList<FatoDoContrato> _fatos = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExtratoVazio))]
    private IReadOnlyList<LinhaDoExtrato> _extrato = [];

    [ObservableProperty]
    private IReadOnlyList<TotalDoContrato> _totais = [];

    public bool Carregada => Locacao is not null;
    public bool EstaAberta => Locacao?.Status == OpcaoDeStatus.Abertas.Valor;
    public bool ExtratoVazio => Carregada && Extrato.Count == 0;

    // A navegação só aceita Action; quem precisa aguardar o carregamento usa CarregarAsync.
    public void Carregar(int id) => _ = CarregarAsync(id);

    public async Task CarregarAsync(int id)
    {
        await ExecutarAsync(async () => Aplicar(await _api.ObterLocacaoAsync(id)));
    }

    [RelayCommand]
    private void VoltarParaLocacoes() => _navegacao.NavegarPara<LocacoesViewModel>();

    [RelayCommand]
    private void RegistrarDevolucao() =>
        _navegacao.NavegarPara<DevolucaoViewModel>(devolucao => devolucao.Carregar(Locacao!.Id));

    [RelayCommand]
    private async Task CancelarLocacaoAsync()
    {
        var confirmado = _dialogos.Confirmar("Cancelar locação", MensagemDeCancelamento, "Cancelar locação");
        if (!confirmado) return;

        await ExecutarAsync(async () => Aplicar(await _api.CancelarLocacaoAsync(Locacao!.Id)));
    }

    private void Aplicar(LocacaoResponse locacao)
    {
        Locacao = locacao;
        Entrada.LocacaoId = locacao.Id;
        Prorrogacao.Locacao = locacao;
        Fatos = MontarFatos(locacao);
        Extrato = locacao.Pagamentos.OrderBy(pagamento => pagamento.DataPagamento).Select(LinhaDoExtrato.De).ToList();
        Totais = MontarTotais(locacao);
    }

    private static List<FatoDoContrato> MontarFatos(LocacaoResponse locacao)
    {
        var fatos = new List<FatoDoContrato>
        {
            new("Cliente", locacao.ClienteNome),
            new("Retirada", locacao.DataRetirada.ToString(FormatoDeData, PtBr)),
            new("Devolução prevista", locacao.DataPrevistaDevolucao.ToString(FormatoDeData, PtBr))
        };
        if (locacao.DataDevolucao is { } devolucao)
            fatos.Add(new("Devolvido em", devolucao.ToString(FormatoDeData, PtBr)));
        fatos.Add(new("Km na retirada", locacao.KmRetirada.ToString("N0", PtBr)));
        if (locacao.KmDevolucao is { } km)
            fatos.Add(new("Km na devolução", km.ToString("N0", PtBr)));
        AdicionarSeHouver(fatos, "Observação da retirada", locacao.ObservacaoRetirada);
        AdicionarSeHouver(fatos, "Observação da devolução", locacao.ObservacaoDevolucao);
        return fatos;
    }

    private static void AdicionarSeHouver(List<FatoDoContrato> fatos, string rotulo, string? texto)
    {
        if (!string.IsNullOrWhiteSpace(texto)) fatos.Add(new(rotulo, texto));
    }

    private static List<TotalDoContrato> MontarTotais(LocacaoResponse locacao)
    {
        var totais = new List<TotalDoContrato> { new("Previsto", locacao.ValorPrevisto) };
        if (locacao.ValorMulta > 0) totais.Add(new("Multa", locacao.ValorMulta));
        totais.Add(new("Total", locacao.ValorTotal));
        totais.Add(new("Pago", locacao.ValorPago));
        return totais;
    }
}
