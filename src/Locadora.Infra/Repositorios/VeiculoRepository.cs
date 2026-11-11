using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class VeiculoRepository(LocadoraDbContext context) : IVeiculoRepository
{
    public Task<Veiculo?> ObterPorIdAsync(int id, CancellationToken ct) =>
        context.Set<Veiculo>().Include(v => v.Categoria).FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<List<Veiculo>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct)
    {
        var consulta = context.Set<Veiculo>().Include(v => v.Categoria).AsNoTracking();

        if (!incluirInativos)
            consulta = consulta.Where(v => v.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var placa = Veiculo.NormalizarPlaca(termo);

            consulta = consulta.Where(v =>
                v.Placa.Contains(placa) || v.Marca.Contains(termo) || v.Modelo.Contains(termo));
        }

        return consulta.OrderBy(v => v.Placa).ToListAsync(ct);
    }

    public Task<List<Veiculo>> ListarDisponiveisAsync(int? categoriaId, CancellationToken ct)
    {
        var consulta = context.Set<Veiculo>()
            .Include(v => v.Categoria)
            .AsNoTracking()
            .Where(v => v.Ativo && v.Status == StatusVeiculo.Disponivel);

        if (categoriaId is not null)
            consulta = consulta.Where(v => v.CategoriaId == categoriaId);

        return consulta.OrderBy(v => v.Placa).ToListAsync(ct);
    }

    public Task<bool> ExistePlacaAsync(string placa, CancellationToken ct) =>
        context.Set<Veiculo>().AnyAsync(v => v.Placa == placa, ct);

    public Task<int> ContarAtivosNaCategoriaAsync(int categoriaId, CancellationToken ct) =>
        context.Set<Veiculo>().CountAsync(v => v.CategoriaId == categoriaId && v.Ativo, ct);

    public void Adicionar(Veiculo veiculo) => context.Set<Veiculo>().Add(veiculo);
}
