using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class DevolucaoViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IApiClient _api;
    private readonly INavegacao _navegacao;
    private int _locacaoId;

    public DevolucaoViewModel(IApiClient api, INavegacao navegacao)
    {
        _api = api;
        _navegacao = navegacao;
        Saidas.CollectionChanged += AoAlterarSaidas;
    }

    public ObservableCollection<SaidaDaDevolucao> Saidas { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Carregada), nameof(DevolucaoPrevista))]
    private LocacaoResponse? _locacao;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Carregada), nameof(HaAtraso), nameof(FraseDoAtraso), nameof(Resumo), nameof(SaldoRestante))]
    [NotifyPropertyChangedFor(nameof(SituacaoDoFechamento), nameof(TextoDoFechamento))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarDevolucaoCommand))]
    private PreviaDevolucaoResponse? _previa;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvisoDoKm))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmarDevolucaoCommand))]
    private string _kmDigitado = "";

    [ObservableProperty]
    private string _observacao = "";

    public bool Carregada => Locacao is not null && Previa is not null;
    public bool HaAtraso => Previa is { DiasAtraso: > 0 };
    public string DevolucaoPrevista => Locacao?.DataPrevistaDevolucao.ToString("dd/MM/yyyy HH:mm", PtBr) ?? "";

    public string FraseDoAtraso => Previa is { DiasAtraso: > 0 } previa
        ? $"{Plural(previa.DiasAtraso, "dia")} de atraso. Multa de {ValorMonetario.Formatar(previa.ValorMulta)}."
        : "";

    public IReadOnlyList<TotalDoContrato> Resumo => Previa is null ? [] : MontarResumo(Previa);

    public decimal SaldoRestante => Previa is null ? 0 : Previa.Saldo - Saidas.Sum(saida => saida.Valor ?? 0);

    public SituacaoDoFechamento SituacaoDoFechamento => SaldoRestante switch
    {
        > 0 => SituacaoDoFechamento.Falta,
        < 0 => SituacaoDoFechamento.Sobra,
        _ => SituacaoDoFechamento.Fechado
    };

    public string TextoDoFechamento => SituacaoDoFechamento switch
    {
        SituacaoDoFechamento.Falta => $"Falta {ValorMonetario.Formatar(SaldoRestante)}",
        SituacaoDoFechamento.Sobra => $"Sobra {ValorMonetario.Formatar(-SaldoRestante)}",
        _ => "Saldo fechado"
    };

    public string? AvisoDoKm => TentarLerKm(out var km) && Locacao is { } locacao && km < locacao.KmRetirada
        ? $"Menor que o km da retirada ({locacao.KmRetirada.ToString("N0", PtBr)})."
        : null;

    public void Carregar(int locacaoId) => _ = CarregarAsync(locacaoId);

    public async Task CarregarAsync(int locacaoId)
    {
        _locacaoId = locacaoId;
        await ExecutarAsync(async () =>
        {
            Locacao = await _api.ObterLocacaoAsync(locacaoId);
            Previa = await _api.ObterPreviaDevolucaoAsync(locacaoId);
        });
    }

    [RelayCommand]
    private void AdicionarSaida() => Saidas.Add(new SaidaDaDevolucao(AtualizarFechamento));

    [RelayCommand]
    private void RemoverSaida(SaidaDaDevolucao saida) => Saidas.Remove(saida);

    [RelayCommand]
    private void VoltarParaContrato() => AbrirContrato();

    [RelayCommand(CanExecute = nameof(PodeConfirmarDevolucao))]
    public async Task ConfirmarDevolucaoAsync()
    {
        await ExecutarAsync(async () =>
        {
            await _api.FinalizarLocacaoAsync(_locacaoId, MontarRequisicao());
            AbrirContrato();
        });
    }

    private bool PodeConfirmarDevolucao() =>
        Carregada && TentarLerKm(out _) && Saidas.All(saida => saida.Valor is not null) && SaldoRestante == 0;

    private void AoAlterarSaidas(object? origem, NotifyCollectionChangedEventArgs evento) => AtualizarFechamento();

    private void AtualizarFechamento()
    {
        OnPropertyChanged(nameof(SaldoRestante));
        OnPropertyChanged(nameof(SituacaoDoFechamento));
        OnPropertyChanged(nameof(TextoDoFechamento));
        ConfirmarDevolucaoCommand.NotifyCanExecuteChanged();
    }

    private FinalizarLocacaoRequest MontarRequisicao()
    {
        TentarLerKm(out var km);
        var saidas = Saidas.Select(saida => saida.ParaRequisicao(saida.Valor!.Value)).ToList();
        var observacao = string.IsNullOrWhiteSpace(Observacao) ? null : Observacao.Trim();
        return new FinalizarLocacaoRequest(km, observacao, saidas);
    }

    private bool TentarLerKm(out int km) =>
        int.TryParse(KmDigitado, NumberStyles.Integer, PtBr, out km);

    private void AbrirContrato() =>
        _navegacao.NavegarPara<LocacaoDetalheViewModel>(detalhe => detalhe.Carregar(_locacaoId));

    private static List<TotalDoContrato> MontarResumo(PreviaDevolucaoResponse previa)
    {
        var resumo = new List<TotalDoContrato> { new("Previsto", previa.ValorPrevisto) };
        if (previa.ValorMulta > 0) resumo.Add(new("Multa", previa.ValorMulta));
        resumo.Add(new("Total", previa.ValorTotal));
        resumo.Add(new("Pago", previa.ValorPago));
        return resumo;
    }

    private static string Plural(int quantidade, string unidade) =>
        quantidade == 1 ? $"1 {unidade}" : $"{quantidade} {unidade}s";
}
