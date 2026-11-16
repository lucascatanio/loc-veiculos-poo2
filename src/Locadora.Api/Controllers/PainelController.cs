using Locadora.Api.Dtos.Painel;
using Locadora.Application.AppServices;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace Locadora.Api.Controllers;

[ApiController]
[Route("api/painel")]
public class PainelController(IPainelAppService appService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PainelResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PainelResponse>> Obter(CancellationToken ct)
    {
        var painel = await appService.ObterAsync(ct);
        return Ok(painel.Adapt<PainelResponse>());
    }
}
