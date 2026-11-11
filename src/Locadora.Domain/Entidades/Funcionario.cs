using Locadora.Domain.Excecoes;
using Locadora.Domain.Interfaces;

namespace Locadora.Domain.Entidades;

public class Funcionario
{
    public const int TamanhoMinimoSenha = 6;

    public int Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public bool Ativo { get; private set; } = true;

    private Funcionario() { }

    public Funcionario(string nome, string email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new DomainException("A senha é obrigatória.");

        DefinirDados(nome, email);
        SenhaHash = senhaHash;
    }

    public bool VerificarSenha(string senha, IHashSenha hash) => hash.Verificar(senha, SenhaHash);

    public void Atualizar(string nome, string email) => DefinirDados(nome, email);

    public void Inativar(int idSolicitante)
    {
        if (Id == idSolicitante)
            throw new DomainException("Você não pode inativar o seu próprio usuário.");

        Ativo = false;
    }

    public void Reativar() => Ativo = true;

    public static void ValidarSenha(string senha)
    {
        if (string.IsNullOrEmpty(senha) || senha.Length < TamanhoMinimoSenha)
            throw new DomainException($"A senha deve ter no mínimo {TamanhoMinimoSenha} caracteres.");
    }

    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private void DefinirDados(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do funcionário é obrigatório.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail do funcionário é obrigatório.");

        Nome = nome.Trim();
        Email = NormalizarEmail(email);
    }
}
