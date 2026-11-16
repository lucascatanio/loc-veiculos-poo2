using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;

namespace Locadora.Domain.Entidades;

public class Pagamento
{
    public int Id { get; private set; }
    public decimal Valor { get; private set; }
    public DateTime DataPagamento { get; private set; }
    public TipoPagamento Tipo { get; private set; }
    public FormaPagamento Forma { get; private set; }

    private Pagamento() { }

    public Pagamento(decimal valor, FormaPagamento forma, TipoPagamento tipo, DateTime dataPagamento)
    {
        if (valor <= 0)
            throw new DomainException("O valor do pagamento deve ser maior que zero.");
        if (decimal.Round(valor, 2) != valor)
            throw new DomainException("O valor do pagamento deve ter no máximo 2 casas decimais.");

        Valor = valor;
        Forma = forma;
        Tipo = tipo;
        DataPagamento = dataPagamento;
    }

    public static Pagamento CriarEntrada(decimal valor, string forma, DateTime dataPagamento) =>
        new(valor, ConverterForma(forma), TipoPagamento.Entrada, dataPagamento);

    // Enum.TryParse sozinho aceita qualquer número ("7"); IsDefined barra o que não é membro do enum.
    private static FormaPagamento ConverterForma(string forma) =>
        Enum.TryParse<FormaPagamento>(forma, ignoreCase: true, out var convertida) && Enum.IsDefined(convertida)
            ? convertida
            : throw new DomainException("Forma de pagamento inválida. Use: Dinheiro, CartaoCredito, CartaoDebito ou Pix.");
}
