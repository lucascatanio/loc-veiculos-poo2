using System.Windows;
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
