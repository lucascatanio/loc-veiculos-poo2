#if DEBUG
using Locadora.Desktop.ViewModels;
using Locadora.Desktop.ViewModels.Locacoes;

namespace Locadora.Desktop.Desenvolvimento;

// Atalhos para abrir o app direto num estado e tirar prints (ignorados em Release):
//   --login <email> <senha>          entra sozinho
//   --tela <Chave|NovaLocacao>       abre a tela do menu ou a nova locação
//   --locacao <id>                   abre o contrato da locação
//   --devolucao <id>                 abre a devolução da locação
//   --preencher-retirada <busca>     preenche a nova locação com o primeiro cliente e veículo encontrados
//   --galeria                        abre a galeria de componentes
public static class AtalhosDeDesenvolvimento
{
    private const string TelaNovaLocacao = "NovaLocacao";
    private const int EsperaDaBuscaMs = 1500;

    public static async Task AplicarAsync(MainViewModel principal, string[] args)
    {
        if (args.Contains("--galeria"))
            new GaleriaComponentes().Show();

        if (!await EntrarAsync(principal, args) || principal.Conteudo is not ShellViewModel shell)
            return;

        if (ValorDoArgumento(args, "--tela") is { } tela)
            AbrirTela(shell, tela);

        if (int.TryParse(ValorDoArgumento(args, "--locacao"), out var locacaoId))
            shell.Navegacao.NavegarPara<LocacaoDetalheViewModel>(contrato => contrato.Carregar(locacaoId));

        if (int.TryParse(ValorDoArgumento(args, "--devolucao"), out var devolucaoId))
            shell.Navegacao.NavegarPara<DevolucaoViewModel>(devolucao => devolucao.Carregar(devolucaoId));

        if (ValorDoArgumento(args, "--preencher-retirada") is { } busca
            && shell.Navegacao.ViewModelAtual is NovaLocacaoViewModel retirada)
            await PreencherRetiradaAsync(retirada, busca);
    }

    private static async Task<bool> EntrarAsync(MainViewModel principal, string[] args)
    {
        var posicao = Array.IndexOf(args, "--login");
        if (posicao < 0 || posicao + 2 >= args.Length || principal.Conteudo is not LoginViewModel login)
            return false;

        login.Email = args[posicao + 1];
        login.Senha = args[posicao + 2];
        await login.EntrarCommand.ExecuteAsync(null);
        return true;
    }

    private static string? ValorDoArgumento(string[] args, string nome)
    {
        var posicao = Array.IndexOf(args, nome);
        return posicao >= 0 && posicao + 1 < args.Length ? args[posicao + 1] : null;
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
