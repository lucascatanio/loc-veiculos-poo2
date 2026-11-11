using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class ClienteService(IClienteRepository clientes) : IClienteService
{
    public async Task GarantirCpfDisponivelAsync(string cpf, CancellationToken ct)
    {
        if (await clientes.ExisteCpfAsync(Cliente.NormalizarCpf(cpf), ct))
            throw new DomainException("Já existe um cliente com este CPF.");
    }
}
