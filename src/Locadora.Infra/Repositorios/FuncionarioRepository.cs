using Locadora.Domain.Entidades;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class FuncionarioRepository(LocadoraDbContext context) : IFuncionarioRepository
{
    public Task<Funcionario?> ObterPorIdAsync(int id, CancellationToken ct) =>
        context.Set<Funcionario>().FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<Funcionario?> ObterPorEmailAsync(string email, CancellationToken ct) =>
        context.Set<Funcionario>().FirstOrDefaultAsync(f => f.Email == email, ct);

    public Task<List<Funcionario>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct)
    {
        var consulta = context.Set<Funcionario>().AsNoTracking();

        if (!incluirInativos)
            consulta = consulta.Where(f => f.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            consulta = consulta.Where(f => f.Nome.Contains(termo) || f.Email.Contains(termo));
        }

        return consulta.OrderBy(f => f.Nome).ToListAsync(ct);
    }

    public Task<bool> ExisteEmailAsync(string email, int? ignorarId, CancellationToken ct) =>
        context.Set<Funcionario>().AnyAsync(f => f.Email == email && f.Id != ignorarId, ct);

    public void Adicionar(Funcionario funcionario) => context.Set<Funcionario>().Add(funcionario);
}
