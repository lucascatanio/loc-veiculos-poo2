using CommunityToolkit.Mvvm.ComponentModel;
using Locadora.Desktop.Servicos;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const string MensagemSessaoEncerrada = "Sua sessão terminou. Entre de novo.";

    private readonly IServiceProvider _provider;
    private readonly SessaoUsuario _sessao;
    private ShellViewModel? _shell;

    public MainViewModel(IServiceProvider provider, SessaoUsuario sessao)
    {
        _provider = provider;
        _sessao = sessao;
        _sessao.SessaoEncerrada += AoEncerrarSessao;
        MostrarLogin(null);
    }

    [ObservableProperty]
    private ViewModelBase? _conteudo;

    private void MostrarLogin(string? aviso)
    {
        var login = _provider.GetRequiredService<LoginViewModel>();
        login.Aviso = aviso;
        login.LoginConcluido += MostrarShell;
        _shell = null;
        Conteudo = login;
    }

    private void MostrarShell()
    {
        _shell = _provider.GetRequiredService<ShellViewModel>();
        _shell.SairSolicitado += AoSair;
        Conteudo = _shell;
    }

    private void AoSair()
    {
        MostrarLogin(null);
        _sessao.Encerrar();
    }

    private void AoEncerrarSessao()
    {
        System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (Conteudo is ShellViewModel) MostrarLogin(MensagemSessaoEncerrada);
        });
    }
}
