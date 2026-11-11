using Locadora.Api.Dtos.Veiculos;
using Locadora.Application.AppServices;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/veiculos")]
public class VeiculosController(IVeiculoAppService appService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<VeiculoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VeiculoResponse>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool incluirInativos, CancellationToken ct)
    {
        var veiculos = await appService.ListarAsync(busca, incluirInativos, ct);
        return Ok(veiculos.Adapt<List<VeiculoResponse>>());
    }

    [HttpGet("disponiveis")]
    [ProducesResponseType<List<VeiculoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VeiculoResponse>>> ListarDisponiveis(
        [FromQuery] int? categoriaId, CancellationToken ct)
    {
        var veiculos = await appService.ListarDisponiveisAsync(categoriaId, ct);
        return Ok(veiculos.Adapt<List<VeiculoResponse>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeiculoResponse>> Obter(int id, CancellationToken ct)
    {
        var veiculo = await appService.ObterAsync(id, ct);
        return Ok(veiculo.Adapt<VeiculoResponse>());
    }

    [HttpPost]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<VeiculoResponse>> Criar(CriarVeiculoRequest request, CancellationToken ct)
    {
        var veiculo = await appService.CriarAsync(
            request.Placa, request.Marca, request.Modelo, request.Ano, request.Cor,
            request.KmAtual, request.CategoriaId, ct);
        var response = veiculo.Adapt<VeiculoResponse>();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<VeiculoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<VeiculoResponse>> Atualizar(
        int id, AtualizarVeiculoRequest request, CancellationToken ct)
    {
        var veiculo = await appService.AtualizarAsync(
            id, request.Marca, request.Modelo, request.Ano, request.Cor, request.CategoriaId, ct);
        return Ok(veiculo.Adapt<VeiculoResponse>());
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

    [HttpPost("{id:int}/manutencao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> EnviarParaManutencao(int id, CancellationToken ct)
    {
        await appService.EnviarParaManutencaoAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/retornar-manutencao")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RetornarDaManutencao(int id, CancellationToken ct)
    {
        await appService.RetornarDaManutencaoAsync(id, ct);
        return NoContent();
    }
}
