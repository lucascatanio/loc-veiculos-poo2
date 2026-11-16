using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class LocacaoRepository(LocadoraDbContext context) : ILocacaoRepository
{
    public Task<List<Locacao>> ListarAsync(StatusLocacao? status, CancellationToken ct)
    {
        var filtro = status ?? StatusLocacao.Aberta;

        return context.Set<Locacao>()
            .Include(l => l.Cliente)
            .Include(l => l.Veiculo)
            .AsNoTracking()
            .Where(l => l.Status == filtro)
            .OrderBy(l => l.DataPrevistaDevolucao)
            .ToListAsync(ct);
    }

    public Task<Locacao?> ObterComDetalhesAsync(int id, CancellationToken ct) =>
        context.Set<Locacao>()
            .Include(l => l.Cliente)
            .Include(l => l.Veiculo)
            .Include(l => l.Pagamentos.OrderBy(p => p.Id))
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<bool> ExisteAbertaParaClienteAsync(int clienteId, CancellationToken ct) =>
        context.Set<Locacao>().AnyAsync(l => l.ClienteId == clienteId && l.Status == StatusLocacao.Aberta, ct);

    public void Adicionar(Locacao locacao) => context.Set<Locacao>().Add(locacao);
}
