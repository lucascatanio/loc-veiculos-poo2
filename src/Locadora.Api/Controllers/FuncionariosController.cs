using Locadora.Api.Auth;
using Locadora.Api.Dtos.Funcionarios;
using Locadora.Application.AppServices;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/funcionarios")]
public class FuncionariosController(IFuncionarioAppService appService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<FuncionarioResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FuncionarioResponse>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool incluirInativos, CancellationToken ct)
    {
        var funcionarios = await appService.ListarAsync(busca, incluirInativos, ct);
        return Ok(funcionarios.Adapt<List<FuncionarioResponse>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<FuncionarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FuncionarioResponse>> Obter(int id, CancellationToken ct)
    {
        var funcionario = await appService.ObterAsync(id, ct);
        return Ok(funcionario.Adapt<FuncionarioResponse>());
    }

    [HttpPost]
    [ProducesResponseType<FuncionarioResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FuncionarioResponse>> Criar(CriarFuncionarioRequest request, CancellationToken ct)
    {
        var funcionario = await appService.CriarAsync(request.Nome, request.Email, request.Senha, ct);
        var response = funcionario.Adapt<FuncionarioResponse>();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<FuncionarioResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FuncionarioResponse>> Atualizar(
        int id, AtualizarFuncionarioRequest request, CancellationToken ct)
    {
        var funcionario = await appService.AtualizarAsync(id, request.Nome, request.Email, ct);
        return Ok(funcionario.Adapt<FuncionarioResponse>());
    }

    [HttpPost("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        await appService.InativarAsync(id, User.ObterIdFuncionario(), ct);
        return NoContent();
    }

    [HttpPost("{id:int}/reativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        await appService.ReativarAsync(id, ct);
        return NoContent();
    }
}
