using System.Text.RegularExpressions;
using Locadora.Domain.Enums;
using Locadora.Domain.Excecoes;

namespace Locadora.Domain.Entidades;

public partial class Veiculo
{
    private const int AnoMinimo = 1950;

    public int Id { get; private set; }
    public string Placa { get; private set; } = null!;
    public string Marca { get; private set; } = null!;
    public string Modelo { get; private set; } = null!;
    public int Ano { get; private set; }
    public string Cor { get; private set; } = null!;
    public int KmAtual { get; private set; }
    public StatusVeiculo Status { get; private set; } = StatusVeiculo.Disponivel;
    public bool Ativo { get; private set; } = true;
    public int CategoriaId { get; private set; }
    public Categoria Categoria { get; private set; } = null!;

    private Veiculo() { }

    public Veiculo(string placa, string marca, string modelo, int ano, string cor, int kmAtual, Categoria categoria)
    {
        var placaNormalizada = NormalizarPlaca(placa);
        if (!PadraoPlaca().IsMatch(placaNormalizada))
            throw new DomainException("Placa inválida.");
        if (kmAtual < 0)
            throw new DomainException("O km atual não pode ser negativo.");

        DefinirDados(marca, modelo, ano, cor, categoria);
        Placa = placaNormalizada;
        KmAtual = kmAtual;
    }

    public void Atualizar(string marca, string modelo, int ano, string cor, Categoria categoria) =>
        DefinirDados(marca, modelo, ano, cor, categoria);

    public bool PodeSerLocado() => Ativo && Status == StatusVeiculo.Disponivel;

    public void Alocar()
    {
        if (!PodeSerLocado())
            throw new DomainException("Veículo indisponível para locação.");

        Status = StatusVeiculo.Locado;
    }

    public void Liberar(int kmDevolucao)
    {
        if (Status != StatusVeiculo.Locado)
            throw new DomainException("O veículo não está locado.");
        if (kmDevolucao < KmAtual)
            throw new DomainException("Km de devolução menor que o km atual do veículo.");

        KmAtual = kmDevolucao;
        Status = StatusVeiculo.Disponivel;
    }

    public void EnviarParaManutencao()
    {
        if (!PodeSerLocado())
            throw new DomainException("Só veículos disponíveis podem ir para manutenção.");

        Status = StatusVeiculo.Manutencao;
    }

    public void RetornarDaManutencao()
    {
        if (Status != StatusVeiculo.Manutencao)
            throw new DomainException("O veículo não está em manutenção.");

        Status = StatusVeiculo.Disponivel;
    }

    public void Inativar()
    {
        if (Status == StatusVeiculo.Locado)
            throw new DomainException("Não é possível inativar um veículo locado.");

        Ativo = false;
    }

    public void Reativar() => Ativo = true;

    public static string NormalizarPlaca(string? placa) =>
        (placa ?? string.Empty).Replace("-", "").Replace(" ", "").ToUpperInvariant();

    // Padrão antigo (ABC1234) e Mercosul (ABC1D23)
    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$")]
    private static partial Regex PadraoPlaca();

    private void DefinirDados(string marca, string modelo, int ano, string cor, Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(marca))
            throw new DomainException("A marca é obrigatória.");
        if (string.IsNullOrWhiteSpace(modelo))
            throw new DomainException("O modelo é obrigatório.");
        if (string.IsNullOrWhiteSpace(cor))
            throw new DomainException("A cor é obrigatória.");
        if (ano < AnoMinimo)
            throw new DomainException($"O ano deve ser maior ou igual a {AnoMinimo}.");

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
        Cor = cor.Trim();
        Categoria = categoria;
        CategoriaId = categoria.Id;
    }
}
