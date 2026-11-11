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
}
