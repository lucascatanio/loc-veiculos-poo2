using CommunityToolkit.Mvvm.Input;

namespace Locadora.Desktop.ViewModels.Locacoes;

// Reserva o lugar do botão para o card que implementa a ação; até lá o botão fica desabilitado.
public static class ComandoIndisponivel
{
    public static IRelayCommand Criar() => new RelayCommand(() => { }, () => false);
}
