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
        IReadOnlyList<(decimal Valor, string Forma)> entradas, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;
        // O sistema grava horário local da loja sem fuso; datas com fuso no JSON são convertidas para ele.
        if (dataPrevista.Kind != DateTimeKind.Unspecified)
            dataPrevista = DateTime.SpecifyKind(dataPrevista.ToLocalTime(), DateTimeKind.Unspecified);

        var entradasDominio = entradas.Select(e => new EntradaPagamento(e.Valor, e.Forma)).ToList();

        var locacao = await locacaoService.AbrirAsync(
            clienteId, veiculoId, agora, dataPrevista, observacao, entradasDominio, ct);

        locacoes.Adicionar(locacao);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    public async Task<Locacao> RegistrarEntradaAsync(int id, decimal valor, string forma, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;

        var locacao = await locacaoService.RegistrarEntradaAsync(id, new EntradaPagamento(valor, forma), agora, ct);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }
}
