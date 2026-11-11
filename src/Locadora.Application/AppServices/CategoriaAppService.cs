using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class CategoriaAppService(
    ICategoriaRepository categorias,
    ICategoriaService categoriaService,
    IUnitOfWork unitOfWork) : ICategoriaAppService
{
    public Task<List<Categoria>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct) =>
        categorias.ListarAsync(busca, incluirInativos, ct);

    public async Task<Categoria> ObterAsync(int id, CancellationToken ct) =>
        await categorias.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Categoria", id);

    public async Task<Categoria> CriarAsync(string nome, decimal valorDiaria, CancellationToken ct)
    {
        await categoriaService.GarantirNomeDisponivelAsync(nome, null, ct);

        var categoria = new Categoria(nome, valorDiaria);
        categorias.Adicionar(categoria);
        await unitOfWork.CommitAsync(ct);

        return categoria;
    }

    public async Task<Categoria> AtualizarAsync(int id, string nome, decimal valorDiaria, CancellationToken ct)
    {
        var categoria = await ObterAsync(id, ct);
        await categoriaService.GarantirNomeDisponivelAsync(nome, id, ct);

        categoria.Atualizar(nome, valorDiaria);
        await unitOfWork.CommitAsync(ct);

        return categoria;
    }

    public async Task InativarAsync(int id, CancellationToken ct)
    {
        var categoria = await ObterAsync(id, ct);

        categoria.Inativar();
        await unitOfWork.CommitAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct)
    {
        var categoria = await ObterAsync(id, ct);

        categoria.Reativar();
        await unitOfWork.CommitAsync(ct);
    }
}
