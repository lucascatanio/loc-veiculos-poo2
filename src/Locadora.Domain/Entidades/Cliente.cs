using Locadora.Domain.Excecoes;

namespace Locadora.Domain.Entidades;

public class Cliente
{
    private const int TamanhoCpf = 11;
    private const int TamanhoCnh = 11;

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Cpf { get; private set; } = null!;
    public string Cnh { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public string? Email { get; private set; }
    public bool Ativo { get; private set; } = true;

    private Cliente() { }

    public Cliente(string nome, string cpf, string cnh, string telefone, string? email)
    {
        var cpfNormalizado = NormalizarCpf(cpf);
        if (cpfNormalizado.Length != TamanhoCpf)
            throw new DomainException("O CPF deve ter 11 dígitos.");

        Cpf = cpfNormalizado;
        DefinirDados(nome, cnh, telefone, email);
    }

    public void Atualizar(string nome, string cnh, string telefone, string? email) =>
        DefinirDados(nome, cnh, telefone, email);

    public void Inativar() => Ativo = false;

    public void Reativar() => Ativo = true;

    public static string NormalizarCpf(string cpf) => SomenteDigitos(cpf);

    private static string SomenteDigitos(string? valor) =>
        new((valor ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    private void DefinirDados(string nome, string cnh, string telefone, string? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do cliente é obrigatório.");

        var cnhNormalizada = SomenteDigitos(cnh);
        if (cnhNormalizada.Length != TamanhoCnh)
            throw new DomainException("A CNH deve ter 11 dígitos.");

        var telefoneNormalizado = SomenteDigitos(telefone);
        if (telefoneNormalizado.Length is not (10 or 11))
            throw new DomainException("O telefone deve ter 10 ou 11 dígitos (com DDD).");

        Nome = nome.Trim();
        Cnh = cnhNormalizada;
        Telefone = telefoneNormalizado;
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}
