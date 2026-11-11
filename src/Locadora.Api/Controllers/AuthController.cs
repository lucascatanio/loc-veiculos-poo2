using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Locadora.Api.Auth;
using Locadora.Api.Dtos.Auth;
using Locadora.Application.AppServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthAppService appService, JwtTokenService tokens) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var funcionario = await appService.LoginAsync(request.Email, request.Senha, ct);

        if (funcionario is null)
            return Problem(
                detail: "E-mail ou senha inválidos.",
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Não autorizado");

        var (token, expiraEm) = tokens.Gerar(funcionario);
        return Ok(new LoginResponse(token, funcionario.Nome, expiraEm));
    }

    [HttpGet("eu")]
    [ProducesResponseType<UsuarioLogadoResponse>(StatusCodes.Status200OK)]
    public ActionResult<UsuarioLogadoResponse> Eu() =>
        Ok(new UsuarioLogadoResponse(
            User.ObterIdFuncionario(),
            User.FindFirstValue(JwtRegisteredClaimNames.Name)!,
            User.FindFirstValue(JwtRegisteredClaimNames.Email)!));
}
