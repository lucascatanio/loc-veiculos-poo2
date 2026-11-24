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
