using Locadora.Application.AppServices;
using Locadora.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFuncionarioService, FuncionarioService>();

        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IFuncionarioAppService, FuncionarioAppService>();

        return services;
    }
}
