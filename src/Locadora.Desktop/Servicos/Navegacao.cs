using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora.Desktop.Servicos;

public partial class Navegacao(IServiceProvider provider) : ObservableObject, INavegacao
{
    [ObservableProperty]
    private ViewModelBase? _viewModelAtual;

    public void NavegarPara<TViewModel>() where TViewModel : ViewModelBase =>
        ViewModelAtual = provider.GetRequiredService<TViewModel>();

    public void NavegarPara<TViewModel>(Action<TViewModel> configurar) where TViewModel : ViewModelBase
    {
        var viewModel = provider.GetRequiredService<TViewModel>();
        configurar(viewModel);
        ViewModelAtual = viewModel;
    }
}
