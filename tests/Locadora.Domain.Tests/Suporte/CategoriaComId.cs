using Locadora.Domain.Entidades;

namespace Locadora.Domain.Tests.Suporte;

public sealed class CategoriaComId : Categoria
{
    public CategoriaComId(int id, string nome, decimal valorDiaria) : base(nome, valorDiaria) => Id = id;
}
