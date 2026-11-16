using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels;

public sealed record ItemMenu(string Chave, string Titulo, Action Abrir);

public partial class ShellViewModel : ViewModelBase
{
    private readonly SessaoUsuario _sessao;

    public ShellViewModel(INavegacao navegacao, SessaoUsuario sessao)
    {
        Navegacao = navegacao;
        _sessao = sessao;
        Itens =
        [
            new("Painel", "Painel", navegacao.NavegarPara<PainelViewModel>),
            new("Locacoes", "Locações", navegacao.NavegarPara<LocacoesViewModel>),
            new("Clientes", "Clientes", navegacao.NavegarPara<ClientesViewModel>),
            new("Veiculos", "Veículos", navegacao.NavegarPara<VeiculosViewModel>),
            new("Categorias", "Categorias", navegacao.NavegarPara<CategoriasViewModel>),
            new("Funcionarios", "Funcionários", navegacao.NavegarPara<FuncionariosViewModel>),
        ];
        ItemSelecionado = Itens[0];
    }

    public INavegacao Navegacao { get; }
    public IReadOnlyList<ItemMenu> Itens { get; }
    public string NomeFuncionario => _sessao.Nome;

    public event Action? SairSolicitado;

    [ObservableProperty]
    private ItemMenu? _itemSelecionado;

    partial void OnItemSelecionadoChanged(ItemMenu? value) => value?.Abrir();

    public void AbrirTela(string chave)
    {
        var item = Itens.FirstOrDefault(i => string.Equals(i.Chave, chave, StringComparison.OrdinalIgnoreCase));
        if (item is not null) ItemSelecionado = item;
    }

    [RelayCommand]
    private void Sair() => SairSolicitado?.Invoke();
}
