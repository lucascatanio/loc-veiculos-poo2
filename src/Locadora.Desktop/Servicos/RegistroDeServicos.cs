using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora.Desktop.Servicos;

public static class RegistroDeServicos
{
    public static IServiceCollection AdicionarServicosDaLocadora(this IServiceCollection servicos, IConfiguration configuracao)
    {
        var urlBase = configuracao["Api:UrlBase"] ?? throw new InvalidOperationException("Api:UrlBase não configurada.");

        servicos.AddSingleton<SessaoUsuario>();
        servicos.AddTransient<AuthHeaderHandler>();
        servicos.AddHttpClient<IApiClient, ApiClient>(http => http.BaseAddress = new Uri(urlBase))
            .AddHttpMessageHandler<AuthHeaderHandler>();

        servicos.AddSingleton<Navegacao>();
        servicos.AddSingleton<INavegacao>(p => p.GetRequiredService<Navegacao>());
        servicos.AddSingleton<IDialogos, Dialogos>();
        return servicos;
    }
}
