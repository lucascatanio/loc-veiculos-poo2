using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Locadora.Domain.Entidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Locadora.Api.Auth;

public class JwtTokenService(IOptions<JwtOpcoes> opcoes)
{
    private readonly JwtOpcoes _opcoes = opcoes.Value;

    public (string Token, DateTime ExpiraEm) Gerar(Funcionario funcionario)
    {
        // Relógio real, não o TimeProvider: em Development ele pode estar deslocado e o
        // validador do JwtBearer usa o relógio do sistema, então o token nasceria expirado.
        var agora = DateTime.UtcNow;
        var expiraEm = agora.AddHours(_opcoes.ExpiracaoHoras);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, funcionario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, funcionario.Nome),
            new Claim(JwtRegisteredClaimNames.Email, funcionario.Email),
        };

        var credenciais = new SigningCredentials(_opcoes.ChaveDeAssinatura(), SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(_opcoes.Emissor, _opcoes.Audiencia, claims, agora, expiraEm, credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expiraEm);
    }
}
