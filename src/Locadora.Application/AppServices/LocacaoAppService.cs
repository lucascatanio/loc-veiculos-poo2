using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class LocacaoAppService(
    ILocacaoRepository locacoes,
    ILocacaoService locacaoService,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : ILocacaoAppService
{
    public Task<List<Locacao>> ListarAsync(StatusLocacao? status, CancellationToken ct) =>
        locacoes.ListarAsync(status, ct);

    public Task<Locacao> ObterAsync(int id, CancellationToken ct) =>
        locacaoService.ObterAsync(id, ct);

    public async Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<EntradaPagamento> entradas, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;
        // O sistema grava horário local da loja sem fuso; datas com fuso no JSON são convertidas para ele.
        if (dataPrevista.Kind != DateTimeKind.Unspecified)
            dataPrevista = DateTime.SpecifyKind(dataPrevista.ToLocalTime(), DateTimeKind.Unspecified);

        var locacao = await locacaoService.AbrirAsync(
            clienteId, veiculoId, agora, dataPrevista, observacao, entradas, ct);

        locacoes.Adicionar(locacao);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    public async Task<Locacao> RegistrarEntradaAsync(int id, EntradaPagamento entrada, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;

        var locacao = await locacaoService.RegistrarEntradaAsync(id, entrada, agora, ct);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }
}
