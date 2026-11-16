using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Locadora.Api.Dtos.Auth;
using Locadora.Api.Dtos.Categorias;
using Locadora.Api.Dtos.Clientes;
using Locadora.Api.Dtos.Funcionarios;
using Locadora.Api.Dtos.Locacoes;
using Locadora.Api.Dtos.Painel;
using Locadora.Api.Dtos.Veiculos;

namespace Locadora.Desktop.Servicos;

public class ApiClient(HttpClient http) : IApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
        Enviar<LoginResponse>(HttpMethod.Post, "api/auth/login", request, ct);

    public Task<UsuarioLogadoResponse> EuAsync(CancellationToken ct = default) =>
        Enviar<UsuarioLogadoResponse>(HttpMethod.Get, "api/auth/eu", null, ct);

    public Task<List<CategoriaResponse>> ListarCategoriasAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default) =>
        Enviar<List<CategoriaResponse>>(HttpMethod.Get, "api/categorias" + FiltroPadrao(busca, incluirInativos), null, ct);

    public Task<CategoriaResponse> ObterCategoriaAsync(int id, CancellationToken ct = default) =>
        Enviar<CategoriaResponse>(HttpMethod.Get, $"api/categorias/{id}", null, ct);

    public Task<CategoriaResponse> CriarCategoriaAsync(CriarCategoriaRequest request, CancellationToken ct = default) =>
        Enviar<CategoriaResponse>(HttpMethod.Post, "api/categorias", request, ct);

    public Task<CategoriaResponse> AtualizarCategoriaAsync(int id, AtualizarCategoriaRequest request, CancellationToken ct = default) =>
        Enviar<CategoriaResponse>(HttpMethod.Put, $"api/categorias/{id}", request, ct);

    public Task InativarCategoriaAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/categorias/{id}/inativar", null, ct);

    public Task ReativarCategoriaAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/categorias/{id}/reativar", null, ct);

    public Task<List<ClienteResponse>> ListarClientesAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default) =>
        Enviar<List<ClienteResponse>>(HttpMethod.Get, "api/clientes" + FiltroPadrao(busca, incluirInativos), null, ct);

    public Task<ClienteResponse> ObterClienteAsync(int id, CancellationToken ct = default) =>
        Enviar<ClienteResponse>(HttpMethod.Get, $"api/clientes/{id}", null, ct);

    public Task<ClienteResponse> CriarClienteAsync(CriarClienteRequest request, CancellationToken ct = default) =>
        Enviar<ClienteResponse>(HttpMethod.Post, "api/clientes", request, ct);

    public Task<ClienteResponse> AtualizarClienteAsync(int id, AtualizarClienteRequest request, CancellationToken ct = default) =>
        Enviar<ClienteResponse>(HttpMethod.Put, $"api/clientes/{id}", request, ct);

    public Task InativarClienteAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/clientes/{id}/inativar", null, ct);

    public Task ReativarClienteAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/clientes/{id}/reativar", null, ct);

    public Task<List<VeiculoResponse>> ListarVeiculosAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default) =>
        Enviar<List<VeiculoResponse>>(HttpMethod.Get, "api/veiculos" + FiltroPadrao(busca, incluirInativos), null, ct);

    public Task<List<VeiculoResponse>> ListarVeiculosDisponiveisAsync(int? categoriaId = null, CancellationToken ct = default) =>
        Enviar<List<VeiculoResponse>>(HttpMethod.Get, "api/veiculos/disponiveis" + Query(("categoriaId", categoriaId?.ToString())), null, ct);

    public Task<VeiculoResponse> ObterVeiculoAsync(int id, CancellationToken ct = default) =>
        Enviar<VeiculoResponse>(HttpMethod.Get, $"api/veiculos/{id}", null, ct);

    public Task<VeiculoResponse> CriarVeiculoAsync(CriarVeiculoRequest request, CancellationToken ct = default) =>
        Enviar<VeiculoResponse>(HttpMethod.Post, "api/veiculos", request, ct);

    public Task<VeiculoResponse> AtualizarVeiculoAsync(int id, AtualizarVeiculoRequest request, CancellationToken ct = default) =>
        Enviar<VeiculoResponse>(HttpMethod.Put, $"api/veiculos/{id}", request, ct);

    public Task InativarVeiculoAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/veiculos/{id}/inativar", null, ct);

    public Task ReativarVeiculoAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/veiculos/{id}/reativar", null, ct);

    public Task EnviarVeiculoParaManutencaoAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/veiculos/{id}/manutencao", null, ct);

    public Task RetornarVeiculoDaManutencaoAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/veiculos/{id}/retornar-manutencao", null, ct);

    public Task<List<FuncionarioResponse>> ListarFuncionariosAsync(string? busca = null, bool incluirInativos = false, CancellationToken ct = default) =>
        Enviar<List<FuncionarioResponse>>(HttpMethod.Get, "api/funcionarios" + FiltroPadrao(busca, incluirInativos), null, ct);

    public Task<FuncionarioResponse> ObterFuncionarioAsync(int id, CancellationToken ct = default) =>
        Enviar<FuncionarioResponse>(HttpMethod.Get, $"api/funcionarios/{id}", null, ct);

    public Task<FuncionarioResponse> CriarFuncionarioAsync(CriarFuncionarioRequest request, CancellationToken ct = default) =>
        Enviar<FuncionarioResponse>(HttpMethod.Post, "api/funcionarios", request, ct);

    public Task<FuncionarioResponse> AtualizarFuncionarioAsync(int id, AtualizarFuncionarioRequest request, CancellationToken ct = default) =>
        Enviar<FuncionarioResponse>(HttpMethod.Put, $"api/funcionarios/{id}", request, ct);

    public Task InativarFuncionarioAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/funcionarios/{id}/inativar", null, ct);

    public Task ReativarFuncionarioAsync(int id, CancellationToken ct = default) =>
        Enviar(HttpMethod.Post, $"api/funcionarios/{id}/reativar", null, ct);

    public Task<List<LocacaoResumoResponse>> ListarLocacoesAsync(string? status = null, CancellationToken ct = default) =>
        Enviar<List<LocacaoResumoResponse>>(HttpMethod.Get, "api/locacoes" + Query(("status", status)), null, ct);

    public Task<LocacaoResponse> ObterLocacaoAsync(int id, CancellationToken ct = default) =>
        Enviar<LocacaoResponse>(HttpMethod.Get, $"api/locacoes/{id}", null, ct);

    public Task<LocacaoResponse> AbrirLocacaoAsync(AbrirLocacaoRequest request, CancellationToken ct = default) =>
        Enviar<LocacaoResponse>(HttpMethod.Post, "api/locacoes", request, ct);

    public Task<LocacaoResponse> RegistrarPagamentoAsync(int locacaoId, PagamentoRequest request, CancellationToken ct = default) =>
        Enviar<LocacaoResponse>(HttpMethod.Post, $"api/locacoes/{locacaoId}/pagamentos", request, ct);

    public Task<PreviaDevolucaoResponse> ObterPreviaDevolucaoAsync(int locacaoId, CancellationToken ct = default) =>
        Enviar<PreviaDevolucaoResponse>(HttpMethod.Get, $"api/locacoes/{locacaoId}/previa-devolucao", null, ct);

    public Task<LocacaoResponse> FinalizarLocacaoAsync(int locacaoId, FinalizarLocacaoRequest request, CancellationToken ct = default) =>
        Enviar<LocacaoResponse>(HttpMethod.Post, $"api/locacoes/{locacaoId}/finalizar", request, ct);

    public Task<LocacaoResponse> ProrrogarLocacaoAsync(int locacaoId, ProrrogarLocacaoRequest request, CancellationToken ct = default) =>
        Enviar<LocacaoResponse>(HttpMethod.Post, $"api/locacoes/{locacaoId}/prorrogar", request, ct);

    public Task<LocacaoResponse> CancelarLocacaoAsync(int locacaoId, CancellationToken ct = default) =>
        Enviar<LocacaoResponse>(HttpMethod.Post, $"api/locacoes/{locacaoId}/cancelar", null, ct);

    public Task<PainelResponse> ObterPainelAsync(CancellationToken ct = default) =>
        Enviar<PainelResponse>(HttpMethod.Get, "api/painel", null, ct);

    private static string FiltroPadrao(string? busca, bool incluirInativos) =>
        Query(("busca", busca), ("incluirInativos", incluirInativos ? "true" : null));

    private static string Query(params (string Nome, string? Valor)[] parametros)
    {
        var pares = parametros
            .Where(p => !string.IsNullOrWhiteSpace(p.Valor))
            .Select(p => $"{p.Nome}={Uri.EscapeDataString(p.Valor!)}")
            .ToList();
        return pares.Count == 0 ? "" : "?" + string.Join("&", pares);
    }

    private async Task<T> Enviar<T>(HttpMethod metodo, string rota, object? corpo, CancellationToken ct)
    {
        using var resposta = await EnviarBrutoAsync(metodo, rota, corpo, ct);
        try
        {
            return (await resposta.Content.ReadFromJsonAsync<T>(Json, ct))!;
        }
        catch (JsonException)
        {
            throw new ApiException((int)resposta.StatusCode, "A API devolveu uma resposta em formato inesperado.", []);
        }
    }

    private async Task Enviar(HttpMethod metodo, string rota, object? corpo, CancellationToken ct)
    {
        using var resposta = await EnviarBrutoAsync(metodo, rota, corpo, ct);
    }

    private async Task<HttpResponseMessage> EnviarBrutoAsync(HttpMethod metodo, string rota, object? corpo, CancellationToken ct)
    {
        using var requisicao = new HttpRequestMessage(metodo, rota);
        if (corpo is not null)
            requisicao.Content = JsonContent.Create(corpo, corpo.GetType(), options: Json);

        HttpResponseMessage resposta;
        try
        {
            resposta = await http.SendAsync(requisicao, ct);
        }
        catch (HttpRequestException)
        {
            throw ApiException.SemConexao(http.BaseAddress);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw ApiException.SemConexao(http.BaseAddress);
        }

        if (resposta.IsSuccessStatusCode)
            return resposta;

        using (resposta)
            throw await ApiException.CriarAsync(resposta, ct);
    }
}
