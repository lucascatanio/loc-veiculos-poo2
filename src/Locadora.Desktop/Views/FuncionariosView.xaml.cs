using System.Windows.Controls;
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Views;

public partial class FuncionariosView : UserControl
{
    public FuncionariosView()
    {
        InitializeComponent();
        Loaded += (_, _) => (DataContext as FuncionariosViewModel)?.CarregarCommand.Execute(null);
    }
}
