using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Tests.Suporte;

namespace Locadora.Domain.Tests.Entidades;

public sealed class PagamentoTests
{
    private const string MensagemFormaInvalida = "inválido para forma de pagamento. Use: Dinheiro, CartaoCredito, CartaoDebito, Pix.";

    public static TheoryData<decimal> ValoresNaoPositivos => new() { 0m, -0.01m, -100m };

    public static TheoryData<decimal> ValoresComMaisDeDuasCasas => new() { 0.001m, 10.005m, 99.999m, 100.123m };

    public static TheoryData<decimal> ValoresValidos => new() { 0.01m, 10.5m, 10.500m, 99.99m, 1_000_000m };

    [Theory]
    [MemberData(nameof(ValoresNaoPositivos))]
    public void Construtor_ValorZeroOuNegativo_Lanca(decimal valor)
    {
        var ex = Assert.Throws<DomainException>(
            () => new Pagamento(valor, FormaPagamento.Pix, TipoPagamento.Entrada, Fabrica.Agora));

        Assert.Equal("O valor do pagamento deve ser maior que zero.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(ValoresComMaisDeDuasCasas))]
    public void Construtor_ValorComMaisDeDuasCasas_Lanca(decimal valor)
    {
        var ex = Assert.Throws<DomainException>(
            () => new Pagamento(valor, FormaPagamento.Pix, TipoPagamento.Entrada, Fabrica.Agora));

        Assert.Equal("O valor do pagamento deve ter no máximo 2 casas decimais.", ex.Message);
    }

    [Theory]
    [MemberData(nameof(ValoresValidos))]
    public void Construtor_ValorComAteDuasCasas_Aceita(decimal valor)
    {
        var pagamento = new Pagamento(valor, FormaPagamento.Pix, TipoPagamento.Entrada, Fabrica.Agora);

        Assert.Equal(valor, pagamento.Valor);
    }

    [Fact]
    public void Construtor_DadosValidos_GuardaCampos()
    {
        var data = Fabrica.Agora.AddHours(5);

        var pagamento = new Pagamento(250.75m, FormaPagamento.CartaoDebito, TipoPagamento.Saida, data);

        Assert.Equal(250.75m, pagamento.Valor);
        Assert.Equal(FormaPagamento.CartaoDebito, pagamento.Forma);
        Assert.Equal(TipoPagamento.Saida, pagamento.Tipo);
        Assert.Equal(data, pagamento.DataPagamento);
    }

    [Fact]
    public void CriarEntrada_DadosValidos_CriaPagamentoDoTipoEntrada()
    {
        var pagamento = Pagamento.CriarEntrada(80m, "Pix", Fabrica.Agora);

        Assert.Equal(TipoPagamento.Entrada, pagamento.Tipo);
        Assert.Equal(80m, pagamento.Valor);
        Assert.Equal(FormaPagamento.Pix, pagamento.Forma);
        Assert.Equal(Fabrica.Agora, pagamento.DataPagamento);
    }

    [Fact]
    public void CriarSaida_DadosValidos_CriaPagamentoDoTipoSaida()
    {
        var pagamento = Pagamento.CriarSaida(80m, "Dinheiro", Fabrica.Agora);

        Assert.Equal(TipoPagamento.Saida, pagamento.Tipo);
        Assert.Equal(80m, pagamento.Valor);
        Assert.Equal(FormaPagamento.Dinheiro, pagamento.Forma);
        Assert.Equal(Fabrica.Agora, pagamento.DataPagamento);
    }

    [Theory]
    [InlineData("Pix", FormaPagamento.Pix)]
    [InlineData("pix", FormaPagamento.Pix)]
    [InlineData("PIX", FormaPagamento.Pix)]
    [InlineData("cartaocredito", FormaPagamento.CartaoCredito)]
    [InlineData("CARTAODEBITO", FormaPagamento.CartaoDebito)]
    [InlineData("dinheiro", FormaPagamento.Dinheiro)]
    public void CriarEntrada_FormaIgnorandoCaixa_ConverteParaEnum(string forma, FormaPagamento esperada)
    {
        var pagamento = Pagamento.CriarEntrada(10m, forma, Fabrica.Agora);

        Assert.Equal(esperada, pagamento.Forma);
    }

    [Theory]
    [InlineData("Pix", FormaPagamento.Pix)]
    [InlineData("pix", FormaPagamento.Pix)]
    [InlineData("PIX", FormaPagamento.Pix)]
    [InlineData("cartaocredito", FormaPagamento.CartaoCredito)]
    [InlineData("CARTAODEBITO", FormaPagamento.CartaoDebito)]
    [InlineData("dinheiro", FormaPagamento.Dinheiro)]
    public void CriarSaida_FormaIgnorandoCaixa_ConverteParaEnum(string forma, FormaPagamento esperada)
    {
        var pagamento = Pagamento.CriarSaida(10m, forma, Fabrica.Agora);

        Assert.Equal(esperada, pagamento.Forma);
    }

    [Theory]
    [InlineData("boleto")]
    [InlineData("7")]
    [InlineData("99")]
    [InlineData("")]
    public void CriarEntrada_FormaInvalida_Lanca(string forma)
    {
        var ex = Assert.Throws<DomainException>(() => Pagamento.CriarEntrada(10m, forma, Fabrica.Agora));

        Assert.Equal($"Valor \"{forma}\" {MensagemFormaInvalida}", ex.Message);
    }

    [Theory]
    [InlineData("boleto")]
    [InlineData("7")]
    [InlineData("99")]
    [InlineData("")]
    public void CriarSaida_FormaInvalida_Lanca(string forma)
    {
        var ex = Assert.Throws<DomainException>(() => Pagamento.CriarSaida(10m, forma, Fabrica.Agora));

        Assert.Equal($"Valor \"{forma}\" {MensagemFormaInvalida}", ex.Message);
    }

    [Fact]
    public void CriarEntrada_ValorInvalido_Lanca()
    {
        var ex = Assert.Throws<DomainException>(() => Pagamento.CriarEntrada(0m, "Pix", Fabrica.Agora));

        Assert.Equal("O valor do pagamento deve ser maior que zero.", ex.Message);
    }

    [Fact]
    public void CriarSaida_ValorInvalido_Lanca()
    {
        var ex = Assert.Throws<DomainException>(() => Pagamento.CriarSaida(10.001m, "Pix", Fabrica.Agora));

        Assert.Equal("O valor do pagamento deve ter no máximo 2 casas decimais.", ex.Message);
    }
}
