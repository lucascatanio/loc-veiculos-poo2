using Locadora.Domain.Entidades;

namespace Locadora.Domain.Tests.Suporte;

public sealed class FuncionarioComId : Funcionario
{
    public FuncionarioComId(int id, string nome, string email, string senhaHash) : base(nome, email, senhaHash) => Id = id;
}
