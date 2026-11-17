using System.Windows.Controls;
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Views;

public partial class ClientesView : UserControl
{
    public ClientesView()
    {
        InitializeComponent();
        Loaded += (_, _) => (DataContext as ClientesViewModel)?.CarregarCommand.Execute(null);
    }
}
