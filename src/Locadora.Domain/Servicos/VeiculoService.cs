using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class VeiculoService(IVeiculoRepository veiculos, ICategoriaRepository categorias) : IVeiculoService
{
    public async Task GarantirPlacaDisponivelAsync(string placa, CancellationToken ct)
    {
        if (await veiculos.ExistePlacaAsync(placa, ct))
            throw new DomainException("Já existe um veículo com esta placa.");
    }

    public async Task<Categoria> ObterCategoriaAtivaAsync(int categoriaId, CancellationToken ct)
    {
        var categoria = await categorias.ObterPorIdAsync(categoriaId, ct)
            ?? throw new NaoEncontradoException("Categoria", categoriaId);

        if (!categoria.Ativo)
            throw new DomainException("A categoria está inativa.");

        return categoria;
    }
}
