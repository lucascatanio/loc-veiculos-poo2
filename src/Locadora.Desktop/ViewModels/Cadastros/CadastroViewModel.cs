using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels.Cadastros;

public abstract partial class CadastroViewModel<TItem> : ViewModelBase where TItem : class
{
    private bool _atualizandoSelecao;
    private bool _recarregarPendente;
    private bool _carregando;

    protected CadastroViewModel(IDialogos dialogos)
    {
        Dialogos = dialogos;
        PropertyChanged += AtualizarComandosQuandoOcupadoMudar;
    }

    protected IDialogos Dialogos { get; }

    public ObservableCollection<TItem> Itens { get; } = [];

    [ObservableProperty]
    private string _textoBusca = "";

    [ObservableProperty]
    private bool _mostrarInativos;

    [ObservableProperty]
    private TItem? _itemSelecionado;

    [ObservableProperty]
    private bool _listaVazia;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Editando))]
    [NotifyPropertyChangedFor(nameof(PodeInativar))]
    [NotifyPropertyChangedFor(nameof(PodeReativar))]
    private int? _idEmEdicao;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeInativar))]
    [NotifyPropertyChangedFor(nameof(PodeReativar))]
    private bool _itemEmEdicaoAtivo;

    public bool Editando => IdEmEdicao is not null;
    public bool PodeInativar => Editando && ItemEmEdicaoAtivo;
    public bool PodeReativar => Editando && !ItemEmEdicaoAtivo;

    protected abstract string NomeDoCadastro { get; }
    protected abstract Task<List<TItem>> ListarAsync(string? busca, bool incluirInativos);
    protected abstract int ObterId(TItem item);
    protected abstract bool ObterAtivo(TItem item);
    protected abstract void PreencherFormulario(TItem? item);
    protected abstract Task<TItem> SalvarNaApiAsync(int? id);
    protected abstract Task InativarNaApiAsync(int id);
    protected abstract Task ReativarNaApiAsync(int id);
    protected abstract string MensagemConfirmarInativacao(TItem item);
    protected abstract string MensagemConfirmarReativacao(TItem item);

    protected virtual bool FormularioValido() => true;

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task CarregarAsync()
    {
        if (_carregando)
        {
            _recarregarPendente = true;
            return;
        }

        await ExecutarAsync(() => RecarregarListaAsync());
    }

    [RelayCommand(CanExecute = nameof(NaoOcupado))]
    private void LimparFormulario()
    {
        MensagemErro = null;
        DefinirSelecaoSemAplicar(null);
        AplicarSelecao(null);
    }

    [RelayCommand(CanExecute = nameof(NaoOcupado))]
    private async Task SalvarAsync()
    {
        if (!FormularioValido()) return;

        await ExecutarAsync(async () =>
        {
            var salvo = await SalvarNaApiAsync(IdEmEdicao);
            await RecarregarListaAsync(ObterId(salvo), limparSeAusente: false);
        });
    }

    [RelayCommand(CanExecute = nameof(PodeExecutarInativar))]
    private async Task InativarAsync()
    {
        if (ItemSelecionado is not { } item) return;

        var confirmado = Dialogos.Confirmar($"Inativar {NomeDoCadastro}", MensagemConfirmarInativacao(item), "Inativar");
        if (!confirmado) return;

        await ExecutarAsync(async () =>
        {
            await InativarNaApiAsync(ObterId(item));
            await RecarregarListaAsync(ObterId(item), limparSeAusente: true);
        });
    }

    [RelayCommand(CanExecute = nameof(PodeExecutarReativar))]
    private async Task ReativarAsync()
    {
        if (ItemSelecionado is not { } item) return;

        var confirmado = Dialogos.Confirmar($"Reativar {NomeDoCadastro}", MensagemConfirmarReativacao(item), "Reativar");
        if (!confirmado) return;

        await ExecutarAsync(async () =>
        {
            await ReativarNaApiAsync(ObterId(item));
            await RecarregarListaAsync(ObterId(item), limparSeAusente: false);
        });
    }

    partial void OnItemSelecionadoChanged(TItem? value)
    {
        if (_atualizandoSelecao) return;

        MensagemErro = null;
        AplicarSelecao(value);
    }

    private async Task RecarregarListaAsync(int? idParaSelecionar = null, bool limparSeAusente = false)
    {
        _carregando = true;
        try
        {
            do
            {
                _recarregarPendente = false;
                await AtualizarItensAsync(idParaSelecionar ?? IdEmEdicao, limparSeAusente);
            }
            while (_recarregarPendente);
        }
        finally
        {
            _carregando = false;
        }
    }

    private async Task AtualizarItensAsync(int? idParaSelecionar, bool limparSeAusente)
    {
        var busca = string.IsNullOrWhiteSpace(TextoBusca) ? null : TextoBusca.Trim();
        var itens = await ListarAsync(busca, MostrarInativos);

        _atualizandoSelecao = true;
        Itens.Clear();
        foreach (var item in itens)
            Itens.Add(item);
        ItemSelecionado = itens.FirstOrDefault(item => ObterId(item) == idParaSelecionar);
        _atualizandoSelecao = false;

        ListaVazia = itens.Count == 0;
        if (ItemSelecionado is not null)
            AplicarSelecao(ItemSelecionado);
        else if (limparSeAusente)
            AplicarSelecao(null);
    }

    private void DefinirSelecaoSemAplicar(TItem? item)
    {
        _atualizandoSelecao = true;
        ItemSelecionado = item;
        _atualizandoSelecao = false;
    }

    private void AplicarSelecao(TItem? item)
    {
        IdEmEdicao = item is null ? null : ObterId(item);
        ItemEmEdicaoAtivo = item is not null && ObterAtivo(item);
        PreencherFormulario(item);
        NotificarComandosDeAcao();
    }

    private bool NaoOcupado() => !Ocupado;
    private bool PodeExecutarInativar() => PodeInativar && !Ocupado;
    private bool PodeExecutarReativar() => PodeReativar && !Ocupado;

    private void AtualizarComandosQuandoOcupadoMudar(object? origem, PropertyChangedEventArgs evento)
    {
        if (evento.PropertyName == nameof(Ocupado))
            NotificarComandosDeAcao();
    }

    private void NotificarComandosDeAcao()
    {
        LimparFormularioCommand.NotifyCanExecuteChanged();
        SalvarCommand.NotifyCanExecuteChanged();
        InativarCommand.NotifyCanExecuteChanged();
        ReativarCommand.NotifyCanExecuteChanged();
    }
}
