using System.Windows.Controls;
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Views;

public partial class CategoriasView : UserControl
{
    public CategoriasView()
    {
        InitializeComponent();
        Loaded += (_, _) => (DataContext as CategoriasViewModel)?.CarregarCommand.Execute(null);
    }
}
