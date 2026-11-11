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
    public async Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataPrevista, string? observacao,
        IReadOnlyList<EntradaPagamento> entradas, CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;
        // O sistema grava horário local da loja sem fuso; datas com fuso no JSON são convertidas para ele.
        if (dataPrevista.Kind != DateTimeKind.Unspecified)
            dataPrevista = DateTime.SpecifyKind(dataPrevista.ToLocalTime(), DateTimeKind.Unspecified);

        var locacao = await locacaoService.AbrirAsync(clienteId, veiculoId, agora, dataPrevista, observacao, ct);

        foreach (var entrada in entradas)
            locacao.RegistrarPagamento(new Pagamento(entrada.Valor, entrada.Forma, TipoPagamento.Entrada, agora));

        locacoes.Adicionar(locacao);
        await unitOfWork.CommitAsync(ct);

        return locacao;
    }
}
