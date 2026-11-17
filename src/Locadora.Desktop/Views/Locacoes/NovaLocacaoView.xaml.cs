using System.Windows;
using System.Windows.Controls;
using Locadora.Desktop.ViewModels.Locacoes;

namespace Locadora.Desktop.Views.Locacoes;

public partial class NovaLocacaoView : UserControl
{
    public NovaLocacaoView() => InitializeComponent();

    private async void AoCarregar(object sender, RoutedEventArgs e)
    {
        if (DataContext is NovaLocacaoViewModel viewModel)
            await viewModel.CarregarOpcoesAsync();
    }
}
