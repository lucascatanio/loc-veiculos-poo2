using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    private const string MensagemErroInesperado = "Ocorreu um erro inesperado. Tente de novo.";

    [ObservableProperty]
    private bool _ocupado;

    [ObservableProperty]
    private string? _mensagemErro;

    // Falha de conexão chega aqui como ApiException de Status 0, já com a mensagem sobre a URL da API.
    protected async Task ExecutarAsync(Func<Task> acao)
    {
        Ocupado = true;
        MensagemErro = null;
        try
        {
            await acao();
        }
        catch (ApiException ex)
        {
            MensagemErro = ex.Detalhe;
        }
        // Qualquer outra falha vira mensagem na tela em vez de derrubar o app.
        catch (Exception)
        {
            MensagemErro = MensagemErroInesperado;
        }
        finally
        {
            Ocupado = false;
        }
    }
}
