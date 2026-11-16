namespace Locadora.Desktop.Servicos;

public class SessaoUsuario
{
    public string? Token { get; private set; }
    public int Id { get; private set; }
    public string Nome { get; private set; } = "";
    public string Email { get; private set; } = "";
    public DateTime? ExpiraEm { get; private set; }

    public bool Autenticada => !string.IsNullOrEmpty(Token);

    // Pode ser disparado fora da thread de UI (a resposta 401 chega no pipeline do HttpClient).
    public event Action? SessaoEncerrada;

    public void Iniciar(string token, int id, string nome, string email, DateTime expiraEm)
    {
        Token = token;
        Id = id;
        Nome = nome;
        Email = email;
        ExpiraEm = expiraEm;
    }

    public void Encerrar()
    {
        Token = null;
        Id = 0;
        Nome = "";
        Email = "";
        ExpiraEm = null;
        SessaoEncerrada?.Invoke();
    }
}
