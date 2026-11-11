using Locadora.Domain.Entidades;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class ClienteRepository(LocadoraDbContext context) : IClienteRepository
{
    public Task<Cliente?> ObterPorIdAsync(int id, CancellationToken ct) =>
        context.Set<Cliente>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<List<Cliente>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct)
    {
        var consulta = context.Set<Cliente>().AsNoTracking();

        if (!incluirInativos)
            consulta = consulta.Where(c => c.Ativo);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            var digitos = Cliente.NormalizarCpf(termo);

            consulta = digitos.Length > 0
                ? consulta.Where(c => c.Nome.Contains(termo) || c.Cpf.Contains(digitos))
                : consulta.Where(c => c.Nome.Contains(termo));
        }

        return consulta.OrderBy(c => c.Nome).ToListAsync(ct);
    }

    public Task<bool> ExisteCpfAsync(string cpf, CancellationToken ct) =>
        context.Set<Cliente>().AnyAsync(c => c.Cpf == cpf, ct);

    public void Adicionar(Cliente cliente) => context.Set<Cliente>().Add(cliente);
}
