using Locadora.Domain.Entidades;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class CategoriaRepository(LocadoraDbContext context) : ICategoriaRepository
{
    public Task<Categoria?> ObterPorIdAsync(int id, CancellationToken ct) =>
        context.Set<Categoria>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Categoria>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct)
    {
        var consulta = context.Set<Categoria>().AsNoTracking();

        if (!incluirInativos)
            consulta = consulta.Where(c => c.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(c => c.Nome.Contains(termo));
        }

        return consulta.OrderBy(c => c.Nome).ToListAsync(ct);
    }

    public void Adicionar(Categoria categoria) => context.Set<Categoria>().Add(categoria);
}
