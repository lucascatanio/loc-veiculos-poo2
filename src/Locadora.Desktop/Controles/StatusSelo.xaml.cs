using System.Windows;
using System.Windows.Controls;

namespace Locadora.Desktop.Controles;

public partial class StatusSelo : UserControl
{
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status), typeof(string), typeof(StatusSelo), new PropertyMetadata(null));

    public string? Status
    {
        get => (string?)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StatusSelo() => InitializeComponent();
}
