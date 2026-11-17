using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Api.Dtos.Funcionarios;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels.Cadastros;
using Locadora.Desktop.ViewModels.Funcionarios;

namespace Locadora.Desktop.ViewModels;

public partial class FuncionariosViewModel(IApiClient api, IDialogos dialogos, SessaoUsuario sessao)
    : CadastroViewModel<FuncionarioDaLista>(dialogos)
{
    public string Titulo => "Funcionários";

    [ObservableProperty]
    private string _nome = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _senha = "";

    [ObservableProperty]
    private bool _editandoOProprioUsuario;

    protected override string NomeDoCadastro => "funcionário";

    protected override async Task<List<FuncionarioDaLista>> ListarAsync(string? busca, bool incluirInativos)
    {
        var funcionarios = await api.ListarFuncionariosAsync(busca, incluirInativos);
        return funcionarios.Select(funcionario => FuncionarioDaLista.De(funcionario, sessao.Id)).ToList();
    }

    protected override int ObterId(FuncionarioDaLista item) => item.Id;

    protected override bool ObterAtivo(FuncionarioDaLista item) => item.Ativo;

    protected override void PreencherFormulario(FuncionarioDaLista? item)
    {
        Nome = item?.Nome ?? "";
        Email = item?.Email ?? "";
        Senha = "";
        EditandoOProprioUsuario = item?.EhUsuarioLogado ?? false;
    }

    protected override async Task<FuncionarioDaLista> SalvarNaApiAsync(int? id)
    {
        var salvo = id is { } existente
            ? await api.AtualizarFuncionarioAsync(existente, new AtualizarFuncionarioRequest(Nome, Email))
            : await api.CriarFuncionarioAsync(new CriarFuncionarioRequest(Nome, Email, Senha));
        return FuncionarioDaLista.De(salvo, sessao.Id);
    }

    protected override Task InativarNaApiAsync(int id) => api.InativarFuncionarioAsync(id);

    protected override Task ReativarNaApiAsync(int id) => api.ReativarFuncionarioAsync(id);

    protected override string MensagemConfirmarInativacao(FuncionarioDaLista item) =>
        $"{item.Nome} perde o acesso ao sistema na hora, mesmo que esteja logado em outra máquina. Você pode reativar o funcionário depois.";

    protected override string MensagemConfirmarReativacao(FuncionarioDaLista item) =>
        $"{item.Nome} volta a poder entrar no sistema com o e-mail e a senha que já tinha.";
}
