using System.Globalization;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;
using Locadora.Domain.Valores;

namespace Locadora.Domain.Entidades;

public class Locacao
{
    public const decimal TAXA_MULTA = 0.20m;
    public const int TOLERANCIA_MINUTOS = 60;
    public const decimal VALOR_MAXIMO = 99_999_999.99m;

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

        var valorPrevisto = CalcularPrevisto(dataRetirada, dataPrevista, veiculo.Categoria.ValorDiaria);
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
        ValorPrevisto = valorPrevisto;
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

    public int DiariasContratadas() => ContarDiarias(DataRetirada, DataPrevistaDevolucao);

    public int CalcularDiasAtraso(DateTime dataDevolucao)
    {
        var excesso = dataDevolucao - DataPrevistaDevolucao;
        return excesso.TotalMinutes <= TOLERANCIA_MINUTOS ? 0 : (int)Math.Ceiling(excesso.TotalHours / 24);
    }

    public decimal CalcularMulta(DateTime dataDevolucao) =>
        Math.Round(CalcularDiasAtraso(dataDevolucao) * ValorDiaria * (1 + TAXA_MULTA), 2, MidpointRounding.AwayFromZero);

    public decimal CalcularValorTotal(DateTime dataDevolucao) => ValorPrevisto + CalcularMulta(dataDevolucao);

    public PreviaDevolucao CalcularPrevia(DateTime dataDevolucao)
    {
        if (Status != StatusLocacao.Aberta)
            throw new DomainException("Só é possível calcular a prévia de locação aberta.");

        var multa = CalcularMulta(dataDevolucao);
        var total = ValorPrevisto + multa;
        return new PreviaDevolucao(
            DiariasContratadas(), CalcularDiasAtraso(dataDevolucao), ValorPrevisto, multa, total, ValorPago, total - ValorPago);
    }

    public void Finalizar(DateTime dataDevolucao, int kmDevolucao, string? observacao, IReadOnlyList<Pagamento> saidas)
    {
        if (Status != StatusLocacao.Aberta)
            throw new DomainException("Só é possível finalizar locação aberta.");
        if (kmDevolucao < KmRetirada)
            throw new DomainException("Km de devolução menor que o de retirada.");
        if (saidas.Any(s => s.Tipo != TipoPagamento.Saida))
            throw new DomainException("Os pagamentos da finalização devem ser do tipo saída.");

        var multa = CalcularMulta(dataDevolucao);
        var total = ValorPrevisto + multa;
        GarantirDentroDoLimite(total);
        var saldo = total - ValorPago;
        var diferenca = saldo - saidas.Sum(s => s.Valor);
        if (diferenca != 0)
        {
            var cultura = new CultureInfo("pt-BR");
            var detalhe = diferenca > 0
                ? $"Falta R$ {diferenca.ToString("N2", cultura)}."
                : $"Sobra R$ {(-diferenca).ToString("N2", cultura)}.";
            throw new DomainException(
                $"Os pagamentos de saída devem fechar o saldo de R$ {saldo.ToString("N2", cultura)}. {detalhe}");
        }

        Veiculo.Liberar(kmDevolucao);

        ValorMulta = multa;
        ValorTotal = total;
        foreach (var saida in saidas)
        {
            _pagamentos.Add(saida);
            ValorPago += saida.Valor;
        }
        DataDevolucao = dataDevolucao;
        KmDevolucao = kmDevolucao;
        ObservacaoDevolucao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        Status = StatusLocacao.Finalizada;
    }

    public void Prorrogar(DateTime agora, DateTime novaDataPrevista)
    {
        if (Status != StatusLocacao.Aberta)
            throw new DomainException("Só é possível prorrogar locação aberta.");
        if (agora > DataPrevistaDevolucao)
            throw new DomainException("Locação vencida não pode ser prorrogada.");
        if (novaDataPrevista <= DataPrevistaDevolucao)
            throw new DomainException("A nova data deve ser posterior à data prevista atual.");

        var novoPrevisto = CalcularPrevisto(DataRetirada, novaDataPrevista, ValorDiaria);

        DataPrevistaDevolucao = novaDataPrevista;
        ValorPrevisto = novoPrevisto;
        ValorTotal = ValorPrevisto;
    }

    public void Cancelar()
    {
        if (Status != StatusLocacao.Aberta)
            throw new DomainException("Só é possível cancelar locação aberta.");

        Veiculo.CancelarAlocacao();
        Status = StatusLocacao.Cancelada;
    }

    private static int ContarDiarias(DateTime inicio, DateTime fim) =>
        Math.Max(1, (int)Math.Ceiling((fim - inicio).TotalHours / 24));

    private static decimal CalcularPrevisto(DateTime inicio, DateTime fim, decimal valorDiaria)
    {
        var previsto = Math.Round(ContarDiarias(inicio, fim) * valorDiaria, 2, MidpointRounding.AwayFromZero);
        GarantirDentroDoLimite(previsto);
        return previsto;
    }

    // As colunas de valor são decimal(10,2); acima disso o banco recusaria e a locação ficaria travada.
    private static void GarantirDentroDoLimite(decimal valor)
    {
        if (valor > VALOR_MAXIMO)
            throw new DomainException(
                $"O valor da locação passaria do limite de R$ {VALOR_MAXIMO.ToString("N2", new CultureInfo("pt-BR"))}. Reduza o período.");
    }
}
