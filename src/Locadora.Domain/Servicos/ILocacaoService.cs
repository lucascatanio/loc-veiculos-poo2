using Locadora.Domain.Entidades;

namespace Locadora.Domain.Servicos;

public interface ILocacaoService
{
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataRetirada, DateTime dataPrevista, string? observacao,
        CancellationToken ct);
}
