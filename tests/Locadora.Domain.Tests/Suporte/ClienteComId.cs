using Locadora.Domain.Entidades;

namespace Locadora.Domain.Tests.Suporte;

public sealed class ClienteComId : Cliente
{
    public ClienteComId(int id, string nome, string cpf, string cnh, string telefone, string? email)
        : base(nome, cpf, cnh, telefone, email) => Id = id;
}
