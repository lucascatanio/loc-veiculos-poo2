using Locadora.Domain.Entidades;

namespace Locadora.Domain.Servicos;

public interface ILocacaoService
{
    Task<Locacao> ObterAsync(int id, CancellationToken ct);
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataRetirada, DateTime dataPrevista, string? observacao,
        IReadOnlyList<EntradaPagamento> entradas, CancellationToken ct);
    Task<Locacao> RegistrarEntradaAsync(int id, EntradaPagamento entrada, DateTime dataPagamento, CancellationToken ct);
}
