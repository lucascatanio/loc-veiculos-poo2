using System.Windows.Controls;
using Locadora.Desktop.ViewModels;

namespace Locadora.Desktop.Views;

public partial class PainelView : UserControl
{
    public PainelView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await ((PainelViewModel)DataContext).CarregarPainelCommand.ExecuteAsync(null);
    }
}
