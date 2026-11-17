#if DEBUG
using Locadora.Desktop.ViewModels;
using Locadora.Desktop.ViewModels.Locacoes;

namespace Locadora.Desktop.Desenvolvimento;

// --login <email> <senha> entra sozinho; --tela <Painel|Locacoes|Clientes|Veiculos|Categorias|Funcionarios> abre direto nela; --locacao <id> abre o contrato dessa locação; --galeria abre a galeria de componentes.
public static class AtalhosDeDesenvolvimento
{
    public static async Task AplicarAsync(MainViewModel principal, string[] args)
    {
        if (args.Contains("--galeria"))
            new GaleriaComponentes().Show();

        var iLogin = Array.IndexOf(args, "--login");
        if (iLogin < 0 || iLogin + 2 >= args.Length || principal.Conteudo is not LoginViewModel login) return;

        login.Email = args[iLogin + 1];
        login.Senha = args[iLogin + 2];
        await login.EntrarCommand.ExecuteAsync(null);

        var iTela = Array.IndexOf(args, "--tela");
        if (iTela >= 0 && iTela + 1 < args.Length && principal.Conteudo is ShellViewModel shell)
            shell.AbrirTela(args[iTela + 1]);

        var iLocacao = Array.IndexOf(args, "--locacao");
        if (iLocacao >= 0 && iLocacao + 1 < args.Length && int.TryParse(args[iLocacao + 1], out var locacaoId) && principal.Conteudo is ShellViewModel aberto)
            aberto.Navegacao.NavegarPara<LocacaoDetalheViewModel>(detalhe => detalhe.Carregar(locacaoId));
    }
}
#endif
