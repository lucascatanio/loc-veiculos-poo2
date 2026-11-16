using Locadora.Domain.Consultas;
using Locadora.Domain.Interfaces;

namespace Locadora.Application.AppServices;

public class PainelAppService(IPainelConsulta consulta, TimeProvider relogio) : IPainelAppService
{
    public async Task<Painel> ObterAsync(CancellationToken ct)
    {
        var agora = relogio.GetLocalNow().DateTime;

        return new Painel
        {
            Atrasadas = await consulta.ListarAtrasadasAsync(agora, ct),
            VencendoHoje = await consulta.ListarVencendoHojeAsync(agora, ct),
            Frota = await consulta.ContarFrotaPorStatusAsync(ct)
        };
    }
}
