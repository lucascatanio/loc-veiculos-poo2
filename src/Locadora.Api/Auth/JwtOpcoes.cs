using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Locadora.Api.Auth;

public class JwtOpcoes
{
    public const string Secao = "Jwt";

    public string Emissor { get; init; } = "";
    public string Audiencia { get; init; } = "";
    public string Chave { get; init; } = "";
    public int ExpiracaoHoras { get; init; } = 8;

    public SymmetricSecurityKey ChaveDeAssinatura() => new(Encoding.UTF8.GetBytes(Chave));
}
