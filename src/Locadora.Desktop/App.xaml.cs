using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Locadora.Desktop.Servicos;
using Locadora.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Locadora.Desktop;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DefinirCulturaBrasileira();
        DispatcherUnhandledException += AoOcorrerErroNaoTratado;
        TaskScheduler.UnobservedTaskException += (_, args) => args.SetObserved();

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = e.Args,
            ContentRootPath = AppContext.BaseDirectory
        });

        builder.Services.AdicionarServicosDaLocadora(builder.Configuration);
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();

        _host = builder.Build();
        await _host.StartAsync();

        _host.Services.GetRequiredService<MainWindow>().Show();
#if DEBUG
        await Desenvolvimento.AtalhosDeDesenvolvimento.AplicarAsync(_host.Services.GetRequiredService<MainViewModel>(), e.Args);
#endif
    }

    // Última rede de segurança: o que escapar dos ViewModels avisa o funcionário em vez de fechar o app.
    private void AoOcorrerErroNaoTratado(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        var dialogos = _host?.Services.GetService<IDialogos>();
        const string titulo = "Algo deu errado";
        const string mensagem = "Ocorreu um erro inesperado. A última ação pode não ter sido concluída; confira e tente de novo.";

        if (dialogos is not null)
            dialogos.Informar(titulo, mensagem);
        else
            MessageBox.Show(mensagem, titulo);
    }

    // Sem isso o WPF formata datas e números (StringFormat) em en-US, independente do Windows.
    private static void DefinirCulturaBrasileira()
    {
        var cultura = new CultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = cultura;
        CultureInfo.DefaultThreadCurrentUICulture = cultura;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultura.IetfLanguageTag)));
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
