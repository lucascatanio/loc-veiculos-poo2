using System.Windows;
using System.Windows.Controls;
using Locadora.Desktop.ViewModels.Locacoes;

namespace Locadora.Desktop.Views.Locacoes;

public partial class LocacoesView : UserControl
{
    public LocacoesView() => InitializeComponent();

    private async void AoCarregar(object sender, RoutedEventArgs e)
    {
        if (DataContext is LocacoesViewModel viewModel)
            await viewModel.CarregarLocacoesAsync();
    }
}
