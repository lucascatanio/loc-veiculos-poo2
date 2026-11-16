using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace Locadora.Desktop.Servicos;

public class AuthHeaderHandler(SessaoUsuario sessao) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (sessao.Autenticada)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessao.Token);

        var resposta = await base.SendAsync(request, cancellationToken);

        // No login, 401 é senha errada e não sessão expirada.
        if (resposta.StatusCode == HttpStatusCode.Unauthorized && !EhLogin(request))
            sessao.Encerrar();

        return resposta;
    }

    private static bool EhLogin(HttpRequestMessage request) =>
        request.RequestUri?.AbsolutePath.EndsWith("/auth/login", StringComparison.OrdinalIgnoreCase) == true;
}
