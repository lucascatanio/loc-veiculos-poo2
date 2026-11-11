using Locadora.Api.Dtos.Locacoes;
using Locadora.Application.AppServices;
using Locadora.Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/locacoes")]
public class LocacoesController(ILocacaoAppService appService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<LocacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LocacaoResponse>> Abrir(AbrirLocacaoRequest request, CancellationToken ct)
    {
        var entradas = new List<EntradaPagamento>();
        foreach (var entrada in request.Entradas ?? [])
        {
            if (!Enum.TryParse<FormaPagamento>(entrada.Forma, ignoreCase: true, out var forma) || !Enum.IsDefined(forma))
            {
                ModelState.AddModelError(nameof(request.Entradas),
                    "Forma de pagamento inválida. Use: Dinheiro, CartaoCredito, CartaoDebito ou Pix.");
                return ValidationProblem(ModelState);
            }
            entradas.Add(new EntradaPagamento(entrada.Valor, forma));
        }

        var locacao = await appService.AbrirAsync(
            request.ClienteId, request.VeiculoId, request.DataPrevistaDevolucao, request.Observacao, entradas, ct);
        var response = locacao.Adapt<LocacaoResponse>();
        return Created($"/api/locacoes/{response.Id}", response);
    }
}
