using Locadora.Api.Dtos.Locacoes;
using Locadora.Domain.Entidades;
using Mapster;

namespace Locadora.Api.Mapeamento;

public class LocacaoMapeamento : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Locacao, LocacaoResponse>()
            .Map(d => d.SaldoDevedor, s => s.SaldoDevedor());
    }
}
