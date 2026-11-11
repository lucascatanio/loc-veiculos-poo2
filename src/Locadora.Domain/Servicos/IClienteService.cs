namespace Locadora.Domain.Servicos;

public interface IClienteService
{
    Task GarantirCpfDisponivelAsync(string cpf, CancellationToken ct);
    Task GarantirQuePodeInativarAsync(int clienteId, CancellationToken ct);
}
