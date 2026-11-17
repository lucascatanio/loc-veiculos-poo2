#if DEBUG
using Locadora.Desktop.ViewModels;
using Locadora.Desktop.ViewModels.Locacoes;

namespace Locadora.Desktop.Desenvolvimento;

// --login <email> <senha> entra sozinho; --tela <Painel|Locacoes|NovaLocacao|Clientes|Veiculos|Categorias|Funcionarios> abre direto nela; --locacao <id> abre o contrato dessa locação; --preencher-retirada <busca do cliente> preenche a nova locação com o primeiro cliente e veículo encontrados; --galeria abre a galeria de componentes.
public static class AtalhosDeDesenvolvimento
{
    private const string TelaNovaLocacao = "NovaLocacao";
    private const int EsperaDaBuscaMs = 1500;

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
            AbrirTela(shell, args[iTela + 1]);

        var iLocacao = Array.IndexOf(args, "--locacao");
        if (iLocacao >= 0 && iLocacao + 1 < args.Length && int.TryParse(args[iLocacao + 1], out var locacaoId) && principal.Conteudo is ShellViewModel aberto)
            aberto.Navegacao.NavegarPara<LocacaoDetalheViewModel>(detalhe => detalhe.Carregar(locacaoId));

        var iPreencher = Array.IndexOf(args, "--preencher-retirada");
        if (iPreencher >= 0 && iPreencher + 1 < args.Length && principal.Conteudo is ShellViewModel { Navegacao.ViewModelAtual: NovaLocacaoViewModel retirada })
            await PreencherRetiradaAsync(retirada, args[iPreencher + 1]);
    }

    private static void AbrirTela(ShellViewModel shell, string chave)
    {
        if (chave == TelaNovaLocacao)
            shell.Navegacao.NavegarPara<NovaLocacaoViewModel>();
        else
            shell.AbrirTela(chave);
    }

    private static async Task PreencherRetiradaAsync(NovaLocacaoViewModel retirada, string buscaDoCliente)
    {
        await retirada.CarregarOpcoesAsync();
        retirada.BuscaDeCliente = buscaDoCliente;
        await Task.Delay(EsperaDaBuscaMs);
        retirada.ClienteDaBuscaSelecionado = retirada.ClientesEncontrados.FirstOrDefault();
        retirada.VeiculoSelecionado = retirada.VeiculosDisponiveis.FirstOrDefault();
        retirada.AdicionarEntradaCommand.Execute(null);
        retirada.Entradas[0].ValorDigitado = "100";
    }
}
#endif
