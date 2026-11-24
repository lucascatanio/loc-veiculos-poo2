using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels.Locacoes;

namespace Locadora.Desktop.ViewModels;

public sealed record ItemMenu(string Chave, string Titulo, Action Abrir, params Type[] Telas);

public partial class ShellViewModel : ViewModelBase
{
    private readonly SessaoUsuario _sessao;
    private bool _sincronizandoMenu;

    public ShellViewModel(INavegacao navegacao, SessaoUsuario sessao)
    {
        Navegacao = navegacao;
        _sessao = sessao;
        Itens =
        [
            new("Painel", "Painel", navegacao.NavegarPara<PainelViewModel>, typeof(PainelViewModel)),
            new("Locacoes", "Locações", navegacao.NavegarPara<LocacoesViewModel>,
                typeof(LocacoesViewModel), typeof(LocacaoDetalheViewModel), typeof(NovaLocacaoViewModel), typeof(DevolucaoViewModel)),
            new("Clientes", "Clientes", navegacao.NavegarPara<ClientesViewModel>, typeof(ClientesViewModel)),
            new("Veiculos", "Veículos", navegacao.NavegarPara<VeiculosViewModel>, typeof(VeiculosViewModel)),
            new("Categorias", "Categorias", navegacao.NavegarPara<CategoriasViewModel>, typeof(CategoriasViewModel)),
            new("Funcionarios", "Funcionários", navegacao.NavegarPara<FuncionariosViewModel>, typeof(FuncionariosViewModel)),
        ];

        if (navegacao is INotifyPropertyChanged telaAtualObservavel)
            telaAtualObservavel.PropertyChanged += MarcarNoMenuATelaAtual;

        ItemSelecionado = Itens[0];
    }

    public INavegacao Navegacao { get; }
    public IReadOnlyList<ItemMenu> Itens { get; }
    public string NomeFuncionario => _sessao.Nome;

    public event Action? SairSolicitado;

    [ObservableProperty]
    private ItemMenu? _itemSelecionado;

    partial void OnItemSelecionadoChanged(ItemMenu? value)
    {
        if (!_sincronizandoMenu)
            value?.Abrir();
    }

    public void AbrirTela(string chave)
    {
        var item = Itens.FirstOrDefault(i => string.Equals(i.Chave, chave, StringComparison.OrdinalIgnoreCase));
        if (item is not null) ItemSelecionado = item;
    }

    [RelayCommand]
    private void Sair() => SairSolicitado?.Invoke();

    private void MarcarNoMenuATelaAtual(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(INavegacao.ViewModelAtual)) return;

        var tela = Navegacao.ViewModelAtual?.GetType();
        var item = Itens.FirstOrDefault(i => i.Telas.Contains(tela));
        if (item is null || item == ItemSelecionado) return;

        _sincronizandoMenu = true;
        ItemSelecionado = item;
        _sincronizandoMenu = false;
    }
}
