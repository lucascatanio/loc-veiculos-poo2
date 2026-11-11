using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class CategoriaService(ICategoriaRepository categorias) : ICategoriaService
{
    public async Task GarantirNomeDisponivelAsync(string nome, int? ignorarId, CancellationToken ct)
    {
        if (await categorias.ExisteNomeAsync(nome.Trim(), ignorarId, ct))
            throw new DomainException("Já existe uma categoria com este nome.");
    }
}
