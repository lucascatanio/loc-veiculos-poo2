using Locadora.Application.AppServices;
using Locadora.Domain.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IFuncionarioService, FuncionarioService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IClienteService, ClienteService>();

        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IFuncionarioAppService, FuncionarioAppService>();
        services.AddScoped<ICategoriaAppService, CategoriaAppService>();
        services.AddScoped<IClienteAppService, ClienteAppService>();

        return services;
    }
}
