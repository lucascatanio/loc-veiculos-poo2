using Locadora.Domain.Interfaces;

namespace Locadora.Infra.Seguranca;

public class BCryptHashSenha : IHashSenha
{
    public string Gerar(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
