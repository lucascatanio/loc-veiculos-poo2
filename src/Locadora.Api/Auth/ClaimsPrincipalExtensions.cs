using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Locadora.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int ObterIdFuncionario(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
