using Locadora.Domain.Entidades;

namespace Locadora.Domain.Servicos;

public interface IVeiculoService
{
    Task GarantirPlacaDisponivelAsync(string placa, CancellationToken ct);
    Task<Categoria> ObterCategoriaAtivaAsync(int categoriaId, CancellationToken ct);
}
