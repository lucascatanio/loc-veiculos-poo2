using System.Windows;
using Locadora.Desktop.Views;

namespace Locadora.Desktop.Servicos;

public class Dialogos : IDialogos
{
    public bool Confirmar(string titulo, string mensagem, string textoConfirmar = "Confirmar") =>
        Exibir(titulo, mensagem, textoConfirmar, "Cancelar");

    public void Informar(string titulo, string mensagem) =>
        Exibir(titulo, mensagem, "OK", null);

    private static bool Exibir(string titulo, string mensagem, string textoConfirmar, string? textoCancelar)
    {
        var janela = new JanelaDialogo(titulo, mensagem, textoConfirmar, textoCancelar)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow
        };
        return janela.ShowDialog() == true;
    }
}
