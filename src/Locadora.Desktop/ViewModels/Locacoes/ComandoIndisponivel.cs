using CommunityToolkit.Mvvm.Input;

namespace Locadora.Desktop.ViewModels.Locacoes;

public static class ComandoIndisponivel
{
    public static IRelayCommand Criar() => new RelayCommand(() => { }, () => false);
}
