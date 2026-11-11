using System.Globalization;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;

namespace Locadora.Domain.Entidades;

public class Locacao
{
    public const decimal TAXA_MULTA = 0.20m;
    public const int TOLERANCIA_MINUTOS = 60;

    private readonly List<Pagamento> _pagamentos = [];

    public int Id { get; private set; }
    public int ClienteId { get; private set; }
    public Cliente Cliente { get; private set; } = null!;
    public int VeiculoId { get; private set; }
    public Veiculo Veiculo { get; private set; } = null!;
    public DateTime DataRetirada { get; private set; }
    public DateTime DataPrevistaDevolucao { get; private set; }
    public DateTime? DataDevolucao { get; private set; }
    public int KmRetirada { get; private set; }
    public int? KmDevolucao { get; private set; }
    public string? ObservacaoRetirada { get; private set; }
    public string? ObservacaoDevolucao { get; private set; }
    public decimal ValorDiaria { get; private set; }
    public decimal ValorPrevisto { get; private set; }
    public decimal ValorMulta { get; private set; }
    public decimal ValorTotal { get; private set; }
    public decimal ValorPago { get; private set; }
    public StatusLocacao Status { get; private set; }
    public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos;

    private Locacao() { }

    public Locacao(Cliente cliente, Veiculo veiculo, DateTime dataRetirada, DateTime dataPrevista, string? observacao)
    {
        if (dataPrevista <= dataRetirada)
            throw new DomainException("A data prevista de devolução deve ser posterior à retirada.");

        veiculo.Alocar();

        Cliente = cliente;
        ClienteId = cliente.Id;
        Veiculo = veiculo;
        VeiculoId = veiculo.Id;
        DataRetirada = dataRetirada;
        DataPrevistaDevolucao = dataPrevista;
        KmRetirada = veiculo.KmAtual;
        ObservacaoRetirada = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        ValorDiaria = veiculo.Categoria.ValorDiaria;
        ValorPrevisto = Math.Round(ContarDiarias(dataRetirada, dataPrevista) * ValorDiaria, 2, MidpointRounding.AwayFromZero);
        ValorTotal = ValorPrevisto;
        ValorMulta = 0;
        ValorPago = 0;
        Status = StatusLocacao.Aberta;
    }

    public decimal SaldoDevedor() => ValorTotal - ValorPago;

    public void RegistrarPagamento(Pagamento pagamento)
    {
        if (Status != StatusLocacao.Aberta)
            throw new DomainException("Só é possível registrar pagamento em locação aberta.");

        if (pagamento.Tipo == TipoPagamento.Entrada && pagamento.Valor > SaldoDevedor())
            throw new DomainException(
                $"O pagamento excede o saldo devedor de R$ {SaldoDevedor().ToString("N2", new CultureInfo("pt-BR"))}.");

        _pagamentos.Add(pagamento);
        ValorPago += pagamento.Valor;
    }

    private static int ContarDiarias(DateTime inicio, DateTime fim) =>
        Math.Max(1, (int)Math.Ceiling((fim - inicio).TotalHours / 24));
}
