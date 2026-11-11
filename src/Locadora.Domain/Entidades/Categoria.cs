using Locadora.Domain.Excecoes;

namespace Locadora.Domain.Entidades;

public class Categoria
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public decimal ValorDiaria { get; private set; }
    public bool Ativo { get; private set; } = true;

    private Categoria() { }

    public Categoria(string nome, decimal valorDiaria) => DefinirDados(nome, valorDiaria);

    public void Atualizar(string nome, decimal valorDiaria) => DefinirDados(nome, valorDiaria);

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;

    private void DefinirDados(string nome, decimal valorDiaria)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da categoria é obrigatório.");
        if (valorDiaria <= 0)
            throw new DomainException("O valor da diária deve ser maior que zero.");
        if (decimal.Round(valorDiaria, 2) != valorDiaria)
            throw new DomainException("O valor da diária deve ter no máximo 2 casas decimais.");

        Nome = nome.Trim();
        ValorDiaria = valorDiaria;
    }
}
