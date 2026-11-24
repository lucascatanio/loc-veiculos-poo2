using Locadora.Domain.Entidades;
using Locadora.Domain.Enums;

namespace Locadora.Domain.Tests.Suporte;

public static class Fabrica
{
    public static readonly DateTime Agora = new(2025, 3, 10, 10, 0, 0);

    public static Categoria Categoria(
        int id = 1, string nome = "SUV", decimal valorDiaria = 100m, bool ativo = true)
    {
        var categoria = new CategoriaComId(id, nome, valorDiaria);
        if (!ativo) categoria.Inativar();
        return categoria;
    }

    public static Cliente Cliente(
        int id = 1,
        string nome = "Maria Silva",
        string cpf = "52998224725",
        string cnh = "12345678901",
        string telefone = "11987654321",
        string? email = "maria@email.com",
        bool ativo = true)
    {
        var cliente = new ClienteComId(id, nome, cpf, cnh, telefone, email);
        if (!ativo) cliente.Inativar();
        return cliente;
    }

    public static Veiculo Veiculo(
        int id = 1,
        string placa = "ABC1D23",
        string marca = "Fiat",
        string modelo = "Argo",
        int ano = 2022,
        string cor = "Branco",
        int kmAtual = 10_000,
        Categoria? categoria = null,
        StatusVeiculo status = StatusVeiculo.Disponivel,
        bool ativo = true)
    {
        var veiculo = new VeiculoComId(id, placa, marca, modelo, ano, cor, kmAtual, categoria ?? Categoria());
        if (status == StatusVeiculo.Locado) veiculo.Alocar();
        if (status == StatusVeiculo.Manutencao) veiculo.EnviarParaManutencao();
        if (!ativo) veiculo.Inativar();
        return veiculo;
    }

    public static Funcionario Funcionario(
        int id = 1,
        string nome = "Ana Souza",
        string email = "ana@locadora.com",
        string senhaHash = "hash-da-senha",
        bool ativo = true)
    {
        var funcionario = new FuncionarioComId(id, nome, email, senhaHash);
        if (!ativo) funcionario.Inativar(idSolicitante: id + 1);
        return funcionario;
    }

    // valorDiaria e kmRetirada vêm da categoria e do veículo; só valem quando o veículo é criado aqui.
    public static Locacao Locacao(
        int id = 1,
        Cliente? cliente = null,
        Veiculo? veiculo = null,
        decimal? valorDiaria = null,
        int? kmRetirada = null,
        DateTime? retirada = null,
        DateTime? prevista = null,
        string? observacao = null)
    {
        if (veiculo is not null && (valorDiaria is not null || kmRetirada is not null))
            throw new ArgumentException("Informe valorDiaria e kmRetirada ou o veículo, não os dois.");

        veiculo ??= Veiculo(
            kmAtual: kmRetirada ?? 10_000,
            categoria: Categoria(valorDiaria: valorDiaria ?? 100m));
        var dataRetirada = retirada ?? Agora;

        return new LocacaoComId(
            id, cliente ?? Cliente(), veiculo, dataRetirada, prevista ?? dataRetirada.AddDays(3), observacao);
    }

    public static Pagamento Entrada(
        decimal valor = 100m, FormaPagamento forma = FormaPagamento.Pix, DateTime? data = null) =>
        new(valor, forma, TipoPagamento.Entrada, data ?? Agora);

    public static Pagamento Saida(
        decimal valor = 100m, FormaPagamento forma = FormaPagamento.Pix, DateTime? data = null) =>
        new(valor, forma, TipoPagamento.Saida, data ?? Agora);
}
