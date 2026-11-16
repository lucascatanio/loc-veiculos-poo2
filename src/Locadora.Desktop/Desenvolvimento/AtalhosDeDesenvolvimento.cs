#if DEBUG
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Desenvolvimento;

// --login <email> <senha> entra sozinho; --tela <Painel|Locacoes|Clientes|Veiculos|Categorias|Funcionarios> abre direto nela.
public static class AtalhosDeDesenvolvimento
{
    public static async Task AplicarAsync(MainViewModel principal, string[] args)
    {
        var iLogin = Array.IndexOf(args, "--login");
        if (iLogin < 0 || iLogin + 2 >= args.Length || principal.Conteudo is not LoginViewModel login) return;

        login.Email = args[iLogin + 1];
        login.Senha = args[iLogin + 2];
        await login.EntrarCommand.ExecuteAsync(null);

        var iTela = Array.IndexOf(args, "--tela");
        if (iTela >= 0 && iTela + 1 < args.Length && principal.Conteudo is ShellViewModel shell)
            shell.AbrirTela(args[iTela + 1]);
    }
}
#endif
