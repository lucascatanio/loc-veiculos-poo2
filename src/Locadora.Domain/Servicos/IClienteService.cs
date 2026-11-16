using Locadora.Domain.Entidades;

namespace Locadora.Domain.Servicos;

public interface IClienteService
{
    Task<Cliente> ObterAsync(int id, CancellationToken ct);
    Task GarantirCpfDisponivelAsync(string cpf, CancellationToken ct);
    Task GarantirQuePodeInativarAsync(int clienteId, CancellationToken ct);
}
