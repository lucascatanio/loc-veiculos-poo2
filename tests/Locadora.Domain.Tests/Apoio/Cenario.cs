using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;

namespace Locadora.Domain.Tests.Apoio;

internal static class Cenario
{
    public static readonly DateTime Retirada = new(2026, 11, 10, 10, 0, 0);
    public static readonly DateTime Prevista = Retirada.AddDays(3);
    public const decimal Diaria = 100m;
    public const decimal PrevistoTresDiarias = 300m;
    public const int KmInicial = 10_000;

    public static Categoria Categoria(decimal diaria = Diaria) => new CategoriaComId(1, diaria);

    public static Cliente Cliente() => new ClienteComId(7);

    public static Veiculo Veiculo(int km = KmInicial, decimal diaria = Diaria) =>
        new VeiculoComId(9, km, Categoria(diaria));

    public static Locacao LocacaoAberta(DateTime? prevista = null, decimal diaria = Diaria) =>
        new(Cliente(), Veiculo(diaria: diaria), Retirada, prevista ?? Prevista, null);

    public static Locacao LocacaoFinalizada()
    {
        var locacao = LocacaoAberta();
        locacao.Finalizar(Prevista, KmInicial + 500, null, [Saida(PrevistoTresDiarias)]);
        return locacao;
    }

    public static Locacao LocacaoCancelada()
    {
        var locacao = LocacaoAberta();
        locacao.Cancelar();
        return locacao;
    }

    public static Pagamento Entrada(decimal valor) =>
        new(valor, FormaPagamento.Pix, TipoPagamento.Entrada, Retirada);

    public static Pagamento Saida(decimal valor) =>
        new(valor, FormaPagamento.Pix, TipoPagamento.Saida, Prevista);
}
