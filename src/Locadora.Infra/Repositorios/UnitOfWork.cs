using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class UnitOfWork(LocadoraDbContext context) : IUnitOfWork
{
    private const int IndiceUnicoViolado = 2601;
    private const int RestricaoUnicaViolada = 2627;

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
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: IndiceUnicoViolado or RestricaoUnicaViolada })
        {
            throw new DomainException("Já existe um registro com esse valor.");
        }
    }
}
