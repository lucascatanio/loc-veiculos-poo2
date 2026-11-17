using System.Windows.Controls;
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Views;

public partial class VeiculosView : UserControl
{
    public VeiculosView()
    {
        InitializeComponent();
        Loaded += (_, _) => (DataContext as VeiculosViewModel)?.IniciarCommand.Execute(null);
    }
}
