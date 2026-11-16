using Locadora.Domain.Consultas;

namespace Locadora.Application.AppServices;

public interface IPainelAppService
{
    Task<Painel> ObterAsync(CancellationToken ct);
}
