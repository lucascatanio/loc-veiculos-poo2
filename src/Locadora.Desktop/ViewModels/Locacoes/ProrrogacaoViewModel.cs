using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class ProrrogacaoViewModel(IApiClient api, Action<LocacaoResponse> aoProrrogar) : ViewModelBase
{
    private const string FormatoDaData = "dd/MM/yyyy";
    private const string FormatoDaHora = "HH:mm";
    private static readonly string[] FormatosParaLerDataEHora = [$"{FormatoDaData} H:mm", $"{FormatoDaData} HH:mm"];
    private const double HorasPorDiaria = 24;
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MotivoIndisponivel), nameof(Previa), nameof(HaPrevia), nameof(AvisoDaData))]
    [NotifyCanExecuteChangedFor(nameof(AbrirCommand))]
    private LocacaoResponse? _locacao;

    [ObservableProperty]
    private bool _aberto;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Previa), nameof(HaPrevia), nameof(AvisoDaData))]
    private string _dataDigitada = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Previa), nameof(HaPrevia), nameof(AvisoDaData))]
    private string _horaDigitada = "";

    public string? MotivoIndisponivel =>
        EstaVencida ? "Locação vencida não pode ser prorrogada. Registre a devolução." : null;

    public PreviaDaProrrogacao? Previa => TentarLerNovaDataValida(out var novaData) ? CalcularPrevia(novaData) : null;

    public bool HaPrevia => Previa is not null;

    public string? AvisoDaData =>
        TentarLerNovaData(out var novaData) && !EhPosteriorAoPrevisto(novaData)
            ? "A nova data deve ser depois da devolução prevista atual."
            : null;

    private bool EstaVencida => Locacao is { } locacao && locacao.DataPrevistaDevolucao < DateTime.Now;

    [RelayCommand(CanExecute = nameof(PodeAbrir))]
    private void Abrir()
    {
        var sugestao = Locacao!.DataPrevistaDevolucao.AddDays(1);
        DataDigitada = sugestao.ToString(FormatoDaData, PtBr);
        HoraDigitada = sugestao.ToString(FormatoDaHora, PtBr);
        Aberto = true;
    }

    [RelayCommand]
    private void Fechar()
    {
        Aberto = false;
        MensagemErro = null;
    }

    [RelayCommand]
    public async Task ProrrogarAsync()
    {
        if (!TentarLerNovaData(out var novaData))
        {
            MensagemErro = "Informe a nova data e a hora, por exemplo 25/12/2026 e 18:00.";
            return;
        }

        await ExecutarAsync(async () =>
        {
            var locacaoProrrogada = await api.ProrrogarLocacaoAsync(Locacao!.Id, new ProrrogarLocacaoRequest(novaData));
            Fechar();
            aoProrrogar(locacaoProrrogada);
        });
    }

    private bool PodeAbrir() => Locacao is not null && !EstaVencida;

    private bool TentarLerNovaData(out DateTime novaData) =>
        DateTime.TryParseExact($"{DataDigitada} {HoraDigitada.Trim()}", FormatosParaLerDataEHora, PtBr, DateTimeStyles.None, out novaData);

    private bool TentarLerNovaDataValida(out DateTime novaData) =>
        TentarLerNovaData(out novaData) && EhPosteriorAoPrevisto(novaData);

    private bool EhPosteriorAoPrevisto(DateTime novaData) =>
        Locacao is { } locacao && novaData > locacao.DataPrevistaDevolucao;

    private PreviaDaProrrogacao CalcularPrevia(DateTime novaData)
    {
        var locacao = Locacao!;
        var horas = (novaData - locacao.DataRetirada).TotalHours;
        var diarias = Math.Max(1, (int)Math.Ceiling(horas / HorasPorDiaria));
        var previsto = Math.Round(diarias * locacao.ValorDiaria, 2, MidpointRounding.AwayFromZero);
        return new PreviaDaProrrogacao(diarias, previsto, previsto - locacao.ValorPago);
    }
}
