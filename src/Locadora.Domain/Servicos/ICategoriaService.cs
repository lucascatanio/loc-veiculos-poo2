namespace Locadora.Domain.Servicos;

public interface ICategoriaService
{
    Task GarantirNomeDisponivelAsync(string nome, int? ignorarId, CancellationToken ct);
}
