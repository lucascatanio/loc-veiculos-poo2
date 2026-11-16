using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class VeiculoService(IVeiculoRepository veiculos, ICategoriaService categoriaService) : IVeiculoService
{
    public async Task<Veiculo> ObterAsync(int id, CancellationToken ct) =>
        await veiculos.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Veículo", id);

    public async Task GarantirPlacaDisponivelAsync(string placa, CancellationToken ct)
    {
        if (await veiculos.ExistePlacaAsync(placa, ct))
            throw new DomainException("Já existe um veículo com esta placa.");
    }

    public async Task<Categoria> ObterCategoriaAtivaAsync(int categoriaId, CancellationToken ct)
    {
        var categoria = await categoriaService.ObterAsync(categoriaId, ct);
        if (!categoria.Ativo)
            throw new DomainException("A categoria está inativa.");

        return categoria;
    }
}
