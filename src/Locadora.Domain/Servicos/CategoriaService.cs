using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class CategoriaService(ICategoriaRepository categorias, IVeiculoRepository veiculos) : ICategoriaService
{
    public async Task GarantirNomeDisponivelAsync(string nome, int? ignorarId, CancellationToken ct)
    {
        if (await categorias.ExisteNomeAsync(nome.Trim(), ignorarId, ct))
            throw new DomainException("Já existe uma categoria com este nome.");
    }

    public async Task GarantirQuePodeInativarAsync(int categoriaId, CancellationToken ct)
    {
        var quantidade = await veiculos.ContarAtivosNaCategoriaAsync(categoriaId, ct);
        if (quantidade > 0)
            throw new DomainException($"Existem {quantidade} veículo(s) ativo(s) nesta categoria.");
    }
}
