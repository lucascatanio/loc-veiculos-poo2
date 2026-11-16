using System.Windows;

namespace Locadora.Desktop;

public partial class MainWindow : Window
{
    private sealed record Amostra(string Placa, string Modelo, string Status, decimal Diaria);

    public MainWindow()
    {
        InitializeComponent();
        Lista.ItemsSource = new Amostra[]
        {
            new("BRA2E19", "Fiat Argo 1.0", "Disponivel", 129.90m),
            new("RIO4F77", "Chevrolet Onix 1.0", "Locado", 139.00m),
            new("QWE1A23", "Jeep Compass", "Manutencao", 289.90m),
            new("KJH9B88", "VW Nivus", "Atrasada", 219.50m),
            new("ABC1D23", "Renault Kwid", "Finalizada", 99.00m),
        };
    }
}
