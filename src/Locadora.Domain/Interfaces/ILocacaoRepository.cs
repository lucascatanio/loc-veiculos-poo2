using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;

namespace Locadora.Domain.Interfaces;

public interface ILocacaoRepository
{
    Task<List<Locacao>> ListarAsync(StatusLocacao status, CancellationToken ct);
    Task<Locacao?> ObterComDetalhesAsync(int id, CancellationToken ct);
    Task<bool> ExisteAbertaParaClienteAsync(int clienteId, CancellationToken ct);
    void Adicionar(Locacao locacao);
}
