using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class VeiculoAppService(
    IVeiculoRepository veiculos,
    IVeiculoService veiculoService,
    IUnitOfWork unitOfWork) : IVeiculoAppService
{
    public Task<List<Veiculo>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct) =>
        veiculos.ListarAsync(busca, incluirInativos, ct);

    public Task<List<Veiculo>> ListarDisponiveisAsync(int? categoriaId, CancellationToken ct) =>
        veiculos.ListarDisponiveisAsync(categoriaId, ct);

    public async Task<Veiculo> ObterAsync(int id, CancellationToken ct) =>
        await veiculos.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Veículo", id);

    public async Task<Veiculo> CriarAsync(
        string placa, string marca, string modelo, int ano, string cor, int kmAtual, int categoriaId,
        CancellationToken ct)
    {
        var categoria = await veiculoService.ObterCategoriaAtivaAsync(categoriaId, ct);

        var veiculo = new Veiculo(placa, marca, modelo, ano, cor, kmAtual, categoria);
        await veiculoService.GarantirPlacaDisponivelAsync(veiculo.Placa, ct);

        veiculos.Adicionar(veiculo);
        await unitOfWork.CommitAsync(ct);

        return veiculo;
    }

    public async Task<Veiculo> AtualizarAsync(
        int id, string marca, string modelo, int ano, string cor, int categoriaId, CancellationToken ct)
    {
        var veiculo = await ObterAsync(id, ct);
        var categoria = await veiculoService.ObterCategoriaAtivaAsync(categoriaId, ct);

        veiculo.Atualizar(marca, modelo, ano, cor, categoria);
        await unitOfWork.CommitAsync(ct);

        return veiculo;
    }

    public async Task InativarAsync(int id, CancellationToken ct)
    {
        var veiculo = await ObterAsync(id, ct);

        veiculo.Inativar();
        await unitOfWork.CommitAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct)
    {
        var veiculo = await ObterAsync(id, ct);
        await veiculoService.ObterCategoriaAtivaAsync(veiculo.CategoriaId, ct);

        veiculo.Reativar();
        await unitOfWork.CommitAsync(ct);
    }

    public async Task EnviarParaManutencaoAsync(int id, CancellationToken ct)
    {
        var veiculo = await ObterAsync(id, ct);

        veiculo.EnviarParaManutencao();
        await unitOfWork.CommitAsync(ct);
    }

    public async Task RetornarDaManutencaoAsync(int id, CancellationToken ct)
    {
        var veiculo = await ObterAsync(id, ct);

        veiculo.RetornarDaManutencao();
        await unitOfWork.CommitAsync(ct);
    }
}
