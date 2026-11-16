using Locadora.Api.Dtos.Auth;
using Locadora.Api.Dtos.Categorias;
using Locadora.Api.Dtos.Clientes;
using Locadora.Api.Dtos.Funcionarios;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Api.Dtos.Painel;
using Locadora.Api.Dtos.Veiculos;

namespace Locadora.Desktop.Servicos;

public interface IApiClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UsuarioLogadoResponse> EuAsync(CancellationToken ct = default);

    Task<List<CategoriaResponse>> ListarCategoriasAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default);
    Task<CategoriaResponse> ObterCategoriaAsync(int id, CancellationToken ct = default);
    Task<CategoriaResponse> CriarCategoriaAsync(CriarCategoriaRequest request, CancellationToken ct = default);
    Task<CategoriaResponse> AtualizarCategoriaAsync(int id, AtualizarCategoriaRequest request, CancellationToken ct = default);
    Task InativarCategoriaAsync(int id, CancellationToken ct = default);
    Task ReativarCategoriaAsync(int id, CancellationToken ct = default);

    Task<List<ClienteResponse>> ListarClientesAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default);
    Task<ClienteResponse> ObterClienteAsync(int id, CancellationToken ct = default);
    Task<ClienteResponse> CriarClienteAsync(CriarClienteRequest request, CancellationToken ct = default);
    Task<ClienteResponse> AtualizarClienteAsync(int id, AtualizarClienteRequest request, CancellationToken ct = default);
    Task InativarClienteAsync(int id, CancellationToken ct = default);
    Task ReativarClienteAsync(int id, CancellationToken ct = default);

    Task<List<VeiculoResponse>> ListarVeiculosAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default);
    Task<List<VeiculoResponse>> ListarVeiculosDisponiveisAsync(int? categoriaId = null, CancellationToken ct = default);
    Task<VeiculoResponse> ObterVeiculoAsync(int id, CancellationToken ct = default);
    Task<VeiculoResponse> CriarVeiculoAsync(CriarVeiculoRequest request, CancellationToken ct = default);
    Task<VeiculoResponse> AtualizarVeiculoAsync(int id, AtualizarVeiculoRequest request, CancellationToken ct = default);
    Task InativarVeiculoAsync(int id, CancellationToken ct = default);
    Task ReativarVeiculoAsync(int id, CancellationToken ct = default);
    Task EnviarVeiculoParaManutencaoAsync(int id, CancellationToken ct = default);
    Task RetornarVeiculoDaManutencaoAsync(int id, CancellationToken ct = default);

    Task<List<FuncionarioResponse>> ListarFuncionariosAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default);
    Task<FuncionarioResponse> ObterFuncionarioAsync(int id, CancellationToken ct = default);
    Task<FuncionarioResponse> CriarFuncionarioAsync(CriarFuncionarioRequest request, CancellationToken ct = default);
    Task<FuncionarioResponse> AtualizarFuncionarioAsync(int id, AtualizarFuncionarioRequest request, CancellationToken ct = default);
    Task InativarFuncionarioAsync(int id, CancellationToken ct = default);
    Task ReativarFuncionarioAsync(int id, CancellationToken ct = default);

    Task<List<LocacaoResumoResponse>> ListarLocacoesAsync(string? status = null, CancellationToken ct = default);
    Task<LocacaoResponse> ObterLocacaoAsync(int id, CancellationToken ct = default);
    Task<LocacaoResponse> AbrirLocacaoAsync(AbrirLocacaoRequest request, CancellationToken ct = default);
    Task<LocacaoResponse> RegistrarPagamentoAsync(int locacaoId, PagamentoRequest request, CancellationToken ct = default);
    Task<PreviaDevolucaoResponse> ObterPreviaDevolucaoAsync(int locacaoId, CancellationToken ct = default);
    Task<LocacaoResponse> FinalizarLocacaoAsync(int locacaoId, FinalizarLocacaoRequest request, CancellationToken ct = default);
    Task<LocacaoResponse> ProrrogarLocacaoAsync(int locacaoId, ProrrogarLocacaoRequest request, CancellationToken ct = default);
    Task<LocacaoResponse> CancelarLocacaoAsync(int locacaoId, CancellationToken ct = default);

    Task<PainelResponse> ObterPainelAsync(CancellationToken ct = default);
}
