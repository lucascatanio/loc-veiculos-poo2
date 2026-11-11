namespace Locadora.Domain.Excecoes;

public class NaoEncontradoException(string entidade, int id)
    : Exception($"{entidade} {id} não encontrado(a).");
