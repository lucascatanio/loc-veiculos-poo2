using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Interfaces;
using Locadora.Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Infra.Repositorios;

public class LocacaoRepository(LocadoraDbContext context) : ILocacaoRepository
{
    public Task<bool> ExisteAbertaParaClienteAsync(int clienteId, CancellationToken ct) =>
        context.Set<Locacao>().AnyAsync(l => l.ClienteId == clienteId && l.Status == StatusLocacao.Aberta, ct);

    public void Adicionar(Locacao locacao) => context.Set<Locacao>().Add(locacao);
}
