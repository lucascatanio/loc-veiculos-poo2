using Locadora.Api.Dtos.Categorias;
using Locadora.Application.AppServices;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController(ICategoriaAppService appService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<CategoriaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoriaResponse>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool incluirInativos, CancellationToken ct)
    {
        var categorias = await appService.ListarAsync(busca, incluirInativos, ct);
        return Ok(categorias.Adapt<List<CategoriaResponse>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponse>> Obter(int id, CancellationToken ct)
    {
        var categoria = await appService.ObterAsync(id, ct);
        return Ok(categoria.Adapt<CategoriaResponse>());
    }

    [HttpPost]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CategoriaResponse>> Criar(CriarCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await appService.CriarAsync(request.Nome, request.ValorDiaria, ct);
        var response = categoria.Adapt<CategoriaResponse>();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<CategoriaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CategoriaResponse>> Atualizar(
        int id, AtualizarCategoriaRequest request, CancellationToken ct)
    {
        var categoria = await appService.AtualizarAsync(id, request.Nome, request.ValorDiaria, ct);
        return Ok(categoria.Adapt<CategoriaResponse>());
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
