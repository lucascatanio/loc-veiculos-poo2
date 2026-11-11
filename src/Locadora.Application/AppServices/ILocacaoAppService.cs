using Locadora.Domain.Entidades;

namespace Locadora.Application.AppServices;

public interface ILocacaoAppService
{
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<EntradaPagamento> entradas, CancellationToken ct);
}
