using Locadora.Desktop.ViewModels;
using Locadora.Desktop.ViewModels.Locacoes;
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
        servicos.AddTransient<LoginViewModel>();
        servicos.AddTransient<ShellViewModel>();
        servicos.AddTransient<PainelViewModel>();
        servicos.AddTransient<LocacoesViewModel>();
        servicos.AddTransient<LocacaoDetalheViewModel>();
        servicos.AddTransient<NovaLocacaoViewModel>();
        servicos.AddTransient<DevolucaoViewModel>();
        servicos.AddTransient<ClientesViewModel>();
        servicos.AddTransient<VeiculosViewModel>();
        servicos.AddTransient<CategoriasViewModel>();
        servicos.AddTransient<FuncionariosViewModel>();
        return servicos;
    }
}
