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
}
