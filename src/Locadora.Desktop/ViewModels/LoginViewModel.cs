using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Locadora.Api.Dtos.Auth;
using Locadora.Desktop.Servicos;

namespace Locadora.Desktop.ViewModels;

public partial class LoginViewModel(IApiClient api, SessaoUsuario sessao) : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EntrarCommand))]
    private string _email = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EntrarCommand))]
    private string _senha = "";

    [ObservableProperty]
    private string? _aviso;

    public event Action? LoginConcluido;

    private bool PodeEntrar() => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Senha);

    [RelayCommand(CanExecute = nameof(PodeEntrar))]
    private Task EntrarAsync() => ExecutarAsync(async () =>
    {
        Aviso = null;
        try
        {
            await IniciarSessaoAsync();
        }
        catch (ApiException ex) when (ex.Status == 401)
        {
            MensagemErro = "E-mail ou senha inválidos.";
            Senha = "";
            return;
        }

        LoginConcluido?.Invoke();
    });

    private async Task IniciarSessaoAsync()
    {
        var login = await api.LoginAsync(new LoginRequest(Email.Trim(), Senha));
        sessao.Iniciar(login.Token, 0, login.Nome, "", login.ExpiraEm);
        try
        {
            var eu = await api.EuAsync();
            sessao.Iniciar(login.Token, eu.Id, eu.Nome, eu.Email, login.ExpiraEm);
        }
        catch (ApiException)
        {
            sessao.Encerrar();
            throw;
        }
    }
}
