namespace Locadora.Domain.Interfaces;

public interface IHashSenha
{
    string Gerar(string senha);
    bool Verificar(string senha, string hash);
}
