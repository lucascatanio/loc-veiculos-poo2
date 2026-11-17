using Locadora.Api.Dtos.Funcionarios;

namespace Locadora.Desktop.ViewModels.Funcionarios;

public record FuncionarioDaLista(int Id, string Nome, string Email, bool Ativo, bool EhUsuarioLogado)
{
    public static FuncionarioDaLista De(FuncionarioResponse funcionario, int idDoUsuarioLogado) =>
        new(funcionario.Id, funcionario.Nome, funcionario.Email, funcionario.Ativo, funcionario.Id == idDoUsuarioLogado);
}
