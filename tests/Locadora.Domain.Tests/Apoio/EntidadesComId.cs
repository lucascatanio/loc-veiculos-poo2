using Locadora.Domain.Entidades;

namespace Locadora.Domain.Tests.Apoio;

// Em produção o Id vem do banco; aqui as subclasses usam o setter protected para fixá-lo.
internal sealed class CategoriaComId : Categoria
{
    public CategoriaComId(int id, decimal diaria) : base("Econômico", diaria) => Id = id;
}

internal sealed class ClienteComId : Cliente
{
    public ClienteComId(int id)
        : base("Ana Souza", "529.982.247-25", "12345678901", "(11) 98765-4321", "ana@email.com") => Id = id;
}

internal sealed class VeiculoComId : Veiculo
{
    public VeiculoComId(int id, int km, Categoria categoria)
        : base("ABC1D23", "Fiat", "Argo", 2024, "Prata", km, categoria) => Id = id;
}
