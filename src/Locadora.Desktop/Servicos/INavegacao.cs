using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Servicos;

public interface INavegacao
{
    ViewModelBase? ViewModelAtual { get; }

    void NavegarPara<TViewModel>() where TViewModel : ViewModelBase;
    void NavegarPara<TViewModel>(Action<TViewModel> configurar) where TViewModel : ViewModelBase;
}
