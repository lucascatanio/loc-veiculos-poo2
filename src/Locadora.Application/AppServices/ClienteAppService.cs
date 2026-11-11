using Locadora.Domain.Entidades;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;
using Locadora.Domain.Servicos;

namespace Locadora.Application.AppServices;

public class ClienteAppService(
    IClienteRepository clientes,
    IClienteService clienteService,
    IUnitOfWork unitOfWork) : IClienteAppService
{
    public Task<List<Cliente>> ListarAsync(string? busca, bool incluirInativos, CancellationToken ct) =>
        clientes.ListarAsync(busca, incluirInativos, ct);

    public async Task<Cliente> ObterAsync(int id, CancellationToken ct) =>
        await clientes.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Cliente", id);

    public async Task<Cliente> CriarAsync(
        string nome, string cpf, string cnh, string telefone, string? email, CancellationToken ct)
    {
        var cliente = new Cliente(nome, cpf, cnh, telefone, email);
        await clienteService.GarantirCpfDisponivelAsync(cliente.Cpf, ct);

        clientes.Adicionar(cliente);
        await unitOfWork.CommitAsync(ct);

        return cliente;
    }

    public async Task<Cliente> AtualizarAsync(
        int id, string nome, string cnh, string telefone, string? email, CancellationToken ct)
    {
        var cliente = await ObterAsync(id, ct);

        cliente.Atualizar(nome, cnh, telefone, email);
        await unitOfWork.CommitAsync(ct);

        return cliente;
    }

    public async Task InativarAsync(int id, CancellationToken ct)
    {
        var cliente = await ObterAsync(id, ct);

        cliente.Inativar();
        await unitOfWork.CommitAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct)
    {
        var cliente = await ObterAsync(id, ct);

        cliente.Reativar();
        await unitOfWork.CommitAsync(ct);
    }
}
