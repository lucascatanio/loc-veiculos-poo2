using Locadora.Api.Dtos.Locacoes;
using Locadora.Application.AppServices;
using Locadora.Domain.Enums;
using Locadora.Domain.Servicos;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/locacoes")]
public class LocacoesController(ILocacaoAppService appService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<LocacaoResumoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<LocacaoResumoResponse>>> Listar(
        [FromQuery] StatusLocacao? status, CancellationToken ct)
    {
        var locacoes = await appService.ListarAsync(status, ct);
        return Ok(locacoes.Adapt<List<LocacaoResumoResponse>>());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<LocacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocacaoResponse>> Obter(int id, CancellationToken ct)
    {
        var locacao = await appService.ObterAsync(id, ct);
        return Ok(locacao.Adapt<LocacaoResponse>());
    }

    [HttpPost]
    [ProducesResponseType<LocacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocacaoResponse>> Abrir(AbrirLocacaoRequest request, CancellationToken ct)
    {
        var entradas = (request.Entradas ?? []).Adapt<List<EntradaPagamento>>();

        var locacao = await appService.AbrirAsync(
            request.ClienteId, request.VeiculoId, request.DataPrevistaDevolucao, request.Observacao, entradas, ct);
        var response = locacao.Adapt<LocacaoResponse>();
        return CreatedAtAction(nameof(Obter), new { id = response.Id }, response);
    }

    [HttpPost("{id:int}/pagamentos")]
    [ProducesResponseType<LocacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocacaoResponse>> RegistrarPagamento(
        int id, PagamentoRequest request, CancellationToken ct)
    {
        var locacao = await appService.RegistrarEntradaAsync(id, request.Adapt<EntradaPagamento>(), ct);
        return Ok(locacao.Adapt<LocacaoResponse>());
    }
}
