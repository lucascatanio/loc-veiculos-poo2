using Locadora.Application.AppServices;
using Locadora.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<FuncionarioService>();
        services.AddScoped<AuthAppService>();
        services.AddScoped<FuncionarioAppService>();

        return services;
    }
}
