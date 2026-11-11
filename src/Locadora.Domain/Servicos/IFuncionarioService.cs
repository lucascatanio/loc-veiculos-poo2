namespace Locadora.Domain.Servicos;

public interface IFuncionarioService
{
    Task GarantirEmailDisponivelAsync(string email, int? ignorarId, CancellationToken ct);
}
