using Locadora.Api.Dtos.Clientes;
using Locadora.Application.AppServices;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController(IClienteAppService appService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<ClienteResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ClienteResponse>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool incluirInativos, CancellationToken ct)
    {
        var clientes = await appService.ListarAsync(busca, incluirInativos, ct);
        return Ok(clientes.Adapt<List<ClienteResponse>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> Obter(int id, CancellationToken ct)
    {
        var cliente = await appService.ObterAsync(id, ct);
        return Ok(cliente.Adapt<ClienteResponse>());
    }

    [HttpPost]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ClienteResponse>> Criar(CriarClienteRequest request, CancellationToken ct)
    {
        var cliente = await appService.CriarAsync(
            request.Nome, request.Cpf, request.Cnh, request.Telefone, request.Email, ct);
        var response = cliente.Adapt<ClienteResponse>();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ClienteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ClienteResponse>> Atualizar(
        int id, AtualizarClienteRequest request, CancellationToken ct)
    {
        var cliente = await appService.AtualizarAsync(
            id, request.Nome, request.Cnh, request.Telefone, request.Email, ct);
        return Ok(cliente.Adapt<ClienteResponse>());
    }

    [HttpPost("{id:int}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Inativar(int id, CancellationToken ct)
    {
        await appService.InativarAsync(id, ct);
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
