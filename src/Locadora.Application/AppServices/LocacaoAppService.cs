using Locadora.Domain.Entidades;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;
using Locadora.Domain.Valores;

namespace Locadora.Application.AppServices;

public class LocacaoAppService(
    ILocacaoRepository locacoes,
    ILocacaoService locacaoService,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : ILocacaoAppService
{
    public Task<List<Locacao>> ListarAsync(string? status, CancellationToken ct) =>
        locacaoService.ListarAsync(status, ct);

    public Task<Locacao> ObterAsync(int id, CancellationToken ct) =>
        locacaoService.ObterAsync(id, ct);

    public async Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<PagamentoInformado> entradas, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;
        dataPrevista = NormalizarFuso(dataPrevista);

        var locacao = await locacaoService.AbrirAsync(
            clienteId, veiculoId, agora, dataPrevista, observacao, entradas, ct);

        locacoes.Adicionar(locacao);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    public async Task<Locacao> RegistrarEntradaAsync(int id, PagamentoInformado entrada, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;

        var locacao = await locacaoService.RegistrarEntradaAsync(id, entrada, agora, ct);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    public Task<PreviaDevolucao> ObterPreviaAsync(int id, CancellationToken ct) =>
        locacaoService.ObterPreviaAsync(id, relogio.GetLocalNow().DateTime, ct);

    public async Task<Locacao> FinalizarAsync(
        int id, int kmDevolucao, string? observacao, IReadOnlyList<PagamentoInformado> saidas, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;

        var locacao = await locacaoService.FinalizarAsync(id, agora, kmDevolucao, observacao, saidas, ct);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    public async Task<Locacao> ProrrogarAsync(int id, DateTime novaDataPrevista, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;
        novaDataPrevista = NormalizarFuso(novaDataPrevista);

        var locacao = await locacaoService.ProrrogarAsync(id, agora, novaDataPrevista, ct);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    public async Task<Locacao> CancelarAsync(int id, CancellationToken ct)
    {
        var locacao = await locacaoService.CancelarAsync(id, ct);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }

    // O sistema grava horário local da loja sem fuso; datas com fuso no JSON são convertidas para ele.
    private static DateTime NormalizarFuso(DateTime data) =>
        data.Kind == DateTimeKind.Unspecified
            ? data
            : DateTime.SpecifyKind(data.ToLocalTime(), DateTimeKind.Unspecified);
}
