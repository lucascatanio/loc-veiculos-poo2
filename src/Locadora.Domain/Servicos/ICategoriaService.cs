using Locadora.Domain.Entidades;

namespace Locadora.Domain.Servicos;

public interface ICategoriaService
{
    Task<Categoria> ObterAsync(int id, CancellationToken ct);
    Task GarantirNomeDisponivelAsync(string nome, int? ignorarId, CancellationToken ct);
    Task GarantirQuePodeInativarAsync(int categoriaId, CancellationToken ct);
}
