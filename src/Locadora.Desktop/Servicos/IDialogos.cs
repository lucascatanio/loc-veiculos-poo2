namespace Locadora.Desktop.Servicos;

public interface IDialogos
{
    bool Confirmar(string titulo, string mensagem, string textoConfirmar = "Confirmar");
    void Informar(string titulo, string mensagem);
}
