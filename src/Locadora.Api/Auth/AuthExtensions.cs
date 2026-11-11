using System.Text;
using Locadora.Application.AppServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Locadora.Api.Auth;

public static class AuthExtensions
{
    public static IServiceCollection AddAutenticacaoJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var secao = configuration.GetSection(JwtOpcoes.Secao);
        var opcoes = secao.Get<JwtOpcoes>();

        if (opcoes is null || string.IsNullOrWhiteSpace(opcoes.Emissor) || string.IsNullOrWhiteSpace(opcoes.Audiencia)
            || Encoding.UTF8.GetByteCount(opcoes.Chave) < 32)
            throw new InvalidOperationException("Seção Jwt incompleta: Emissor, Audiencia e Chave (mínimo 32 bytes) são obrigatórios.");

        services.Configure<JwtOpcoes>(secao);
        services.AddSingleton<JwtTokenService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                // O token vale 8h; sem esta checagem, um funcionário inativado continuaria usando a API até expirar.
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async contexto =>
                    {
                        var auth = contexto.HttpContext.RequestServices.GetRequiredService<IAuthAppService>();
                        var id = contexto.Principal!.ObterIdFuncionario();

                        if (!await auth.EstaAtivoAsync(id, contexto.HttpContext.RequestAborted))
                            contexto.Fail("Funcionário inativo.");
                    },
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = opcoes.Emissor,
                    ValidateAudience = true,
                    ValidAudience = opcoes.Audiencia,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = opcoes.ChaveDeAssinatura(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    public static void AddSwaggerComBearer(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Informe o token retornado por POST /api/auth/login.",
        });

        options.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", documento)] = [],
        });
    }
}
