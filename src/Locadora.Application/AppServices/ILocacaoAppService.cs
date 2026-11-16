using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;

namespace Locadora.Application.AppServices;

public interface ILocacaoAppService
{
    Task<List<Locacao>> ListarAsync(StatusLocacao? status, CancellationToken ct);
    Task<Locacao> ObterAsync(int id, CancellationToken ct);
    Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<(decimal Valor, string Forma)> entradas, CancellationToken ct);
    Task<Locacao> RegistrarEntradaAsync(int id, decimal valor, string forma, CancellationToken ct);
}
