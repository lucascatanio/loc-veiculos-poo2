using Locadora.Domain.Consultas;

namespace Locadora.Domain.Interfaces;

public interface IPainelConsulta
{
    Task<List<LocacaoAtrasadaItem>> ListarAtrasadasAsync(DateTime agora, CancellationToken ct);
    Task<List<LocacaoVencendoItem>> ListarVencendoHojeAsync(DateTime agora, CancellationToken ct);
    Task<List<ContadorFrotaItem>> ContarFrotaPorStatusAsync(CancellationToken ct);
}
