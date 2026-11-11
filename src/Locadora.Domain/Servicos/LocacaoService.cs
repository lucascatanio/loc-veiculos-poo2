using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Servicos;

public class LocacaoService(IClienteRepository clientes, IVeiculoRepository veiculos) : ILocacaoService
{
    public async Task<Locacao> AbrirAsync(
        int clienteId, int veiculoId, DateTime dataRetirada, DateTime dataPrevista, string? observacao,
        CancellationToken ct)
    {
        var cliente = await clientes.ObterPorIdAsync(clienteId, ct)
            ?? throw new NaoEncontradoException("Cliente", clienteId);
        if (!cliente.Ativo)
            throw new DomainException("O cliente está inativo.");

        var veiculo = await veiculos.ObterPorIdAsync(veiculoId, ct)
            ?? throw new NaoEncontradoException("Veículo", veiculoId);
        if (!veiculo.PodeSerLocado())
            throw new DomainException("Veículo indisponível para locação.");

        return new Locacao(cliente, veiculo, dataRetirada, dataPrevista, observacao);
    }
}
