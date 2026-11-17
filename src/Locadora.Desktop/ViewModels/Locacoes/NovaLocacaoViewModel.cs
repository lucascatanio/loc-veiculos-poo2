using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Clientes;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Locacoes;

public partial class NovaLocacaoViewModel : ViewModelBase
{
    private const int AtrasoDaBuscaMs = 300;
    private const int MinimoDeCaracteresDaBusca = 2;
    private const int MaximoDeClientesNaLista = 5;
    private const int StatusDeConflito = 409;
    private const int StatusDeRegraDeNegocio = 422;
    private const string FormatoParaExibirHora = "HH:mm";
    private const string FormatoParaLerHora = @"hh\:mm";
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IApiClient _api;
    private readonly INavegacao _navegacao;
    private Dictionary<int, decimal> _diariaPorCategoria = [];

    public NovaLocacaoViewModel(IApiClient api, INavegacao navegacao)
    {
        _api = api;
        _navegacao = navegacao;
        var devolucaoPadrao = HoraCheiaAnterior(DateTime.Now.AddDays(1));
        _dataDevolucao = devolucaoPadrao.Date;
        _horaDevolucao = devolucaoPadrao.ToString(FormatoParaExibirHora, PtBr);
        Entradas.CollectionChanged += (_, _) => AtualizarResumo();
        AtualizarResumo();
    }

    public ObservableCollection<EntradaDaRetirada> Entradas { get; } = [];

    [ObservableProperty]
    private string _buscaDeCliente = "";

    [ObservableProperty]
    private IReadOnlyList<ClienteResponse> _clientesEncontrados = [];

    [ObservableProperty]
    private bool _nenhumClienteEncontrado;

    [ObservableProperty]
    private ClienteResponse? _clienteDaBuscaSelecionado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemClienteEscolhido))]
    private ClienteResponse? _clienteEscolhido;

    [ObservableProperty]
    private IReadOnlyList<OpcaoDeCategoria> _categorias = [OpcaoDeCategoria.Todas];

    [ObservableProperty]
    private OpcaoDeCategoria _categoriaSelecionada = OpcaoDeCategoria.Todas;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SemVeiculosDisponiveis))]
    private IReadOnlyList<VeiculoParaRetirada> _veiculosDisponiveis = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SemVeiculosDisponiveis))]
    private bool _carregouVeiculos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TemVeiculoEscolhido))]
    private VeiculoParaRetirada? _veiculoSelecionado;

    [ObservableProperty]
    private DateTime? _dataDevolucao;

    [ObservableProperty]
    private string _horaDevolucao;

    [ObservableProperty]
    private string _observacao = "";

    [ObservableProperty]
    private ResumoDaRetirada _resumo = ResumoDaRetirada.Inicial;

    [ObservableProperty]
    private string? _erroDaHora;

    public bool TemClienteEscolhido => ClienteEscolhido is not null;
    public bool TemVeiculoEscolhido => VeiculoSelecionado is not null;
    public bool SemVeiculosDisponiveis => CarregouVeiculos && VeiculosDisponiveis.Count == 0;

    public async Task CarregarOpcoesAsync()
    {
        await ExecutarAsync(async () =>
        {
            await CarregarCategoriasAsync();
            await AtualizarVeiculosDisponiveisAsync();
        });
    }

    [RelayCommand]
    private void TrocarCliente() => ClienteEscolhido = null;

    [RelayCommand]
    private void AdicionarEntrada() => Entradas.Add(new EntradaDaRetirada(AtualizarResumo));

    [RelayCommand]
    private void RemoverEntrada(EntradaDaRetirada entrada) => Entradas.Remove(entrada);

    [RelayCommand]
    private void VoltarParaLocacoes() => _navegacao.NavegarPara<LocacoesViewModel>();

    [RelayCommand(CanExecute = nameof(PodeRegistrarRetirada))]
    public async Task RegistrarRetiradaAsync()
    {
        if (!TentarMontarRequisicao(out var requisicao)) return;

        await ExecutarAsync(async () =>
        {
            var locacao = await AbrirLocacaoRecarregandoSeConflitarAsync(requisicao);
            _navegacao.NavegarPara<LocacaoDetalheViewModel>(contrato => contrato.Carregar(locacao.Id));
        });
    }

    private bool PodeRegistrarRetirada() =>
        ClienteEscolhido is not null && VeiculoSelecionado is not null && LerDevolucaoPrevista() is not null;

    partial void OnBuscaDeClienteChanged(string value) => _ = BuscarClientesAsync(value);

    partial void OnClienteDaBuscaSelecionadoChanged(ClienteResponse? value)
    {
        if (value is null) return;
        ClienteEscolhido = value;
        ClienteDaBuscaSelecionado = null;
        BuscaDeCliente = "";
    }

    partial void OnCategoriaSelecionadaChanged(OpcaoDeCategoria value) =>
        _ = ExecutarAsync(AtualizarVeiculosDisponiveisAsync);

    partial void OnVeiculoSelecionadoChanged(VeiculoParaRetirada? value) => AtualizarResumo();

    partial void OnClienteEscolhidoChanged(ClienteResponse? value) => AtualizarResumo();

    partial void OnDataDevolucaoChanged(DateTime? value) => AtualizarResumo();

    partial void OnHoraDevolucaoChanged(string value) => AtualizarResumo();

    private async Task BuscarClientesAsync(string busca)
    {
        ClientesEncontrados = [];
        NenhumClienteEncontrado = false;
        var termo = busca.Trim();
        if (termo.Length < MinimoDeCaracteresDaBusca) return;

        await Task.Delay(AtrasoDaBuscaMs);
        if (busca != BuscaDeCliente) return;

        await ExecutarAsync(async () =>
        {
            var clientes = await _api.ListarClientesAsync(termo, incluirInativos: false);
            ClientesEncontrados = clientes.Take(MaximoDeClientesNaLista).ToList();
            NenhumClienteEncontrado = clientes.Count == 0;
        });
    }

    private async Task CarregarCategoriasAsync()
    {
        var categorias = await _api.ListarCategoriasAsync(incluirInativos: true);
        _diariaPorCategoria = categorias.ToDictionary(categoria => categoria.Id, categoria => categoria.ValorDiaria);
        var ativas = categorias
            .Where(categoria => categoria.Ativo)
            .Select(categoria => new OpcaoDeCategoria(categoria.Id, categoria.Nome));
        Categorias = [OpcaoDeCategoria.Todas, .. ativas];
    }

    private async Task AtualizarVeiculosDisponiveisAsync()
    {
        var escolhidoAntes = VeiculoSelecionado?.Id;
        var veiculos = await _api.ListarVeiculosDisponiveisAsync(CategoriaSelecionada.Id);
        VeiculosDisponiveis = veiculos
            .Select(veiculo => VeiculoParaRetirada.De(veiculo, _diariaPorCategoria.GetValueOrDefault(veiculo.CategoriaId)))
            .ToList();
        VeiculoSelecionado = VeiculosDisponiveis.FirstOrDefault(veiculo => veiculo.Id == escolhidoAntes);
        CarregouVeiculos = true;
    }

    // Outro funcionário pode ter locado o veículo entre a listagem e o clique: a API recusa e a lista é atualizada.
    private async Task<LocacaoResponse> AbrirLocacaoRecarregandoSeConflitarAsync(AbrirLocacaoRequest requisicao)
    {
        try
        {
            return await _api.AbrirLocacaoAsync(requisicao);
        }
        catch (ApiException ex) when (ex.Status is StatusDeConflito or StatusDeRegraDeNegocio)
        {
            await AtualizarVeiculosDisponiveisAsync();
            throw;
        }
    }

    private bool TentarMontarRequisicao(out AbrirLocacaoRequest requisicao)
    {
        requisicao = null!;
        var posicaoInvalida = Entradas.ToList().FindIndex(entrada => entrada.Preenchida && entrada.Valor is null);
        if (posicaoInvalida >= 0)
        {
            MensagemErro = $"Informe o valor da entrada {posicaoInvalida + 1}, por exemplo 100,50.";
            return false;
        }

        var pagamentos = Entradas
            .Where(entrada => entrada.Preenchida)
            .Select(entrada => new PagamentoRequest(entrada.Valor!.Value, entrada.FormaSelecionada.Valor))
            .ToList();
        var observacao = string.IsNullOrWhiteSpace(Observacao) ? null : Observacao.Trim();
        requisicao = new AbrirLocacaoRequest(
            ClienteEscolhido!.Id, VeiculoSelecionado!.Id, LerDevolucaoPrevista()!.Value, observacao, pagamentos);
        return true;
    }

    private DateTime? LerDevolucaoPrevista()
    {
        if (DataDevolucao is not { } data) return null;
        var horaValida = TimeSpan.TryParseExact(HoraDevolucao.Trim(), FormatoParaLerHora, PtBr, out var hora);
        return horaValida ? data.Date + hora : null;
    }

    private void AtualizarResumo()
    {
        var devolucaoPrevista = LerDevolucaoPrevista();
        var totalDasEntradas = Entradas.Sum(entrada => entrada.Valor ?? 0);
        Resumo = ResumoDaRetirada.Prever(ClienteEscolhido?.Nome, VeiculoSelecionado, devolucaoPrevista, totalDasEntradas, DateTime.Now);
        ErroDaHora = devolucaoPrevista is null && DataDevolucao is not null ? "Use o formato HH:mm, por exemplo 14:30." : null;
        RegistrarRetiradaCommand.NotifyCanExecuteChanged();
    }

    // Arredonda para baixo: para cima passaria de 24h e o padrão já nasceria com 2 diárias.
    private static DateTime HoraCheiaAnterior(DateTime momento) =>
        new DateTime(momento.Year, momento.Month, momento.Day, momento.Hour, 0, 0);
}
