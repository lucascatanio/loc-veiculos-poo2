using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Categorias;
using Locadora.Api.Dtos.Veiculos;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels.Cadastros;
using Locadora.Desktop.ViewModels.Veiculos;

namespace Locadora.Desktop.ViewModels;

public partial class VeiculosViewModel : CadastroViewModel<VeiculoResponse>
{
    private const string StatusDisponivel = "Disponivel";
    private const string StatusLocado = "Locado";
    private const string StatusManutencao = "Manutencao";

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IApiClient _api;

    public VeiculosViewModel(IApiClient api, IDialogos dialogos) : base(dialogos)
    {
        _api = api;
        _filtroSelecionado = FiltrosDeStatus[0];
        ItensFiltrados = CollectionViewSource.GetDefaultView(Itens);
        ItensFiltrados.Filter = StatusCorrespondeAoFiltro;
        Itens.CollectionChanged += (_, _) => AtualizarListaSemResultados();
        PropertyChanged += AtualizarAcoesQuandoOcupadoMudar;
    }

    public string Titulo => "Veículos";

    public ICollectionView ItensFiltrados { get; }

    public IReadOnlyList<FiltroDeStatus> FiltrosDeStatus { get; } =
    [
        new("Todos", null),
        new("Disponível", StatusDisponivel),
        new("Locado", StatusLocado),
        new("Manutenção", StatusManutencao)
    ];

    public IReadOnlyList<CategoriaResponse> CategoriasAtivas { get; private set; } = [];

    [ObservableProperty]
    private FiltroDeStatus _filtroSelecionado;

    [ObservableProperty]
    private bool _listaSemResultados;

    [ObservableProperty]
    private string _placa = "";

    [ObservableProperty]
    private string _marca = "";

    [ObservableProperty]
    private string _modelo = "";

    [ObservableProperty]
    private string _anoTexto = "";

    [ObservableProperty]
    private string _cor = "";

    [ObservableProperty]
    private string _kmInicialTexto = "";

    [ObservableProperty]
    private int _kmAtual;

    [ObservableProperty]
    private int? _categoriaId;

    [ObservableProperty]
    private string? _erroAno;

    [ObservableProperty]
    private string? _erroKm;

    [ObservableProperty]
    private string? _erroCategoria;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EnviarParaManutencaoCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetornarDaManutencaoCommand))]
    private string? _statusEmEdicao;

    protected override string NomeDoCadastro => "veículo";

    [RelayCommand]
    private async Task IniciarAsync()
    {
        await ExecutarAsync(CarregarCategoriasAtivasAsync);
        await CarregarCommand.ExecuteAsync(null);
    }

    [RelayCommand(CanExecute = nameof(PodeEnviarParaManutencao))]
    private async Task EnviarParaManutencaoAsync()
    {
        if (ItemSelecionado is not { } veiculo) return;

        var mensagem = $"O veículo {veiculo.Placa} deixa de ficar disponível para locação até retornar da manutenção.";
        if (!Dialogos.Confirmar("Enviar para manutenção", mensagem, "Enviar")) return;

        await ExecutarMudancaDeStatusAsync(() => _api.EnviarVeiculoParaManutencaoAsync(veiculo.Id));
    }

    [RelayCommand(CanExecute = nameof(PodeRetornarDaManutencao))]
    private async Task RetornarDaManutencaoAsync()
    {
        if (ItemSelecionado is not { } veiculo) return;

        var mensagem = $"O veículo {veiculo.Placa} volta a ficar disponível para locação.";
        if (!Dialogos.Confirmar("Retornar da manutenção", mensagem, "Retornar")) return;

        await ExecutarMudancaDeStatusAsync(() => _api.RetornarVeiculoDaManutencaoAsync(veiculo.Id));
    }

    protected override Task<List<VeiculoResponse>> ListarAsync(string? busca, bool incluirInativos) =>
        _api.ListarVeiculosAsync(busca, incluirInativos);

    protected override int ObterId(VeiculoResponse item) => item.Id;

    protected override bool ObterAtivo(VeiculoResponse item) => item.Ativo;

    protected override void PreencherFormulario(VeiculoResponse? item)
    {
        Placa = item?.Placa ?? "";
        Marca = item?.Marca ?? "";
        Modelo = item?.Modelo ?? "";
        AnoTexto = item?.Ano.ToString() ?? "";
        Cor = item?.Cor ?? "";
        KmInicialTexto = "";
        KmAtual = item?.KmAtual ?? 0;
        CategoriaId = item?.CategoriaId;
        StatusEmEdicao = item?.Status;
        LimparErrosDeCampo();
    }

    protected override bool FormularioValido()
    {
        ErroAno = TentarConverterInteiro(AnoTexto, out _) ? null : "Informe o ano com números, como 2022.";
        ErroKm = Editando || TentarConverterInteiro(KmInicialTexto, out _) ? null : "Informe a quilometragem com números, como 12000.";
        ErroCategoria = CategoriaId is null ? "Escolha a categoria." : null;
        return ErroAno is null && ErroKm is null && ErroCategoria is null;
    }

    protected override Task<VeiculoResponse> SalvarNaApiAsync(int? id)
    {
        TentarConverterInteiro(AnoTexto, out var ano);
        TentarConverterInteiro(KmInicialTexto, out var kmInicial);
        var categoriaId = CategoriaId!.Value;

        return id is { } existente
            ? _api.AtualizarVeiculoAsync(existente, new AtualizarVeiculoRequest(Marca, Modelo, ano, Cor, categoriaId))
            : _api.CriarVeiculoAsync(new CriarVeiculoRequest(Placa, Marca, Modelo, ano, Cor, kmInicial, categoriaId));
    }

    protected override Task InativarNaApiAsync(int id) => _api.InativarVeiculoAsync(id);

    protected override Task ReativarNaApiAsync(int id) => _api.ReativarVeiculoAsync(id);

    protected override string MensagemConfirmarInativacao(VeiculoResponse item) =>
        $"O veículo {item.Placa} deixa de aparecer para novas locações. Veículo locado não pode ser inativado. Você pode reativá-lo depois.";

    protected override string MensagemConfirmarReativacao(VeiculoResponse item) =>
        $"O veículo {item.Placa} volta a ficar disponível para locação.";

    partial void OnFiltroSelecionadoChanged(FiltroDeStatus value)
    {
        ItensFiltrados.Refresh();
        AtualizarListaSemResultados();
    }

    private async Task CarregarCategoriasAtivasAsync()
    {
        CategoriasAtivas = await _api.ListarCategoriasAsync();
        OnPropertyChanged(nameof(CategoriasAtivas));
    }

    private async Task ExecutarMudancaDeStatusAsync(Func<Task> acao)
    {
        await ExecutarAsync(acao);
        if (MensagemErro is null)
            await CarregarCommand.ExecuteAsync(null);
    }

    private bool PodeEnviarParaManutencao() => !Ocupado && EstaComStatus(StatusDisponivel);

    private bool PodeRetornarDaManutencao() => !Ocupado && EstaComStatus(StatusManutencao);

    private bool EstaComStatus(string status) =>
        Editando && string.Equals(StatusEmEdicao, status, StringComparison.OrdinalIgnoreCase);

    private bool StatusCorrespondeAoFiltro(object item) =>
        FiltroSelecionado.Status is not { } status
        || string.Equals(((VeiculoResponse)item).Status, status, StringComparison.OrdinalIgnoreCase);

    private void AtualizarListaSemResultados() => ListaSemResultados = ItensFiltrados.IsEmpty;

    private void LimparErrosDeCampo()
    {
        ErroAno = null;
        ErroKm = null;
        ErroCategoria = null;
    }

    private void AtualizarAcoesQuandoOcupadoMudar(object? origem, PropertyChangedEventArgs evento)
    {
        if (evento.PropertyName != nameof(Ocupado)) return;

        EnviarParaManutencaoCommand.NotifyCanExecuteChanged();
        RetornarDaManutencaoCommand.NotifyCanExecuteChanged();
    }

    private static bool TentarConverterInteiro(string texto, out int valor) =>
        int.TryParse(texto.Trim(), NumberStyles.None, PtBr, out valor);
}
