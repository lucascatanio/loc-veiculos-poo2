using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Valores;

namespace Locadora.Domain.Servicos;

public class LocacaoService(
    ILocacaoRepository locacoes,
    IClienteService clienteService,
    IVeiculoService veiculoService) : ILocacaoService
{
    public Task<List<Locacao>> ListarAsync(string? status, CancellationToken ct)
    {
        var filtro = string.IsNullOrWhiteSpace(status)
            ? StatusLocacao.Aberta
            : ConversorEnum.Converter<StatusLocacao>(status, "status");

        return locacoes.ListarAsync(filtro, ct);
    }

    public async Task<Locacao> ObterAsync(int id, CancellationToken ct) =>
        await locacoes.ObterComDetalhesAsync(id, ct) ?? throw new NaoEncontradoException("Locação", id);

    public async Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataRetirada, DateTime dataPrevista, string? observacao,
        IReadOnlyList<EntradaPagamento> entradas, CancellationToken ct)
    {
        var cliente = await clienteService.ObterAsync(clienteId, ct);
        if (!cliente.Ativo)
            throw new DomainException("O cliente está inativo.");

        var veiculo = await veiculoService.ObterAsync(veiculoId, ct);
        if (!veiculo.PodeSerLocado())
            throw new DomainException("Veículo indisponível para locação.");

        var locacao = new Locacao(cliente, veiculo, dataRetirada, dataPrevista, observacao);

        foreach (var entrada in entradas)
            locacao.RegistrarPagamento(Pagamento.CriarEntrada(entrada.Valor, entrada.Forma, dataRetirada));

        return locacao;
    }

    public async Task<Locacao> RegistrarEntradaAsync(
        int id, EntradaPagamento entrada, DateTime dataPagamento, CancellationToken ct)
    {
        var locacao = await ObterAsync(id, ct);

        locacao.RegistrarPagamento(Pagamento.CriarEntrada(entrada.Valor, entrada.Forma, dataPagamento));

        return locacao;
    }
}
