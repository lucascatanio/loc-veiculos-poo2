using Locadora.Domain.Entidades;

namespace Locadora.Domain.Interfaces;

public interface ILocacaoRepository
{
    Task<bool> ExisteAbertaParaClienteAsync(int clienteId, CancellationToken ct);
    void Adicionar(Locacao locacao);
}
