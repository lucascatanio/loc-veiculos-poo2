using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class UnitOfWork(LocadoraDbContext context) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflitoConcorrenciaException();
        }
    }
}
