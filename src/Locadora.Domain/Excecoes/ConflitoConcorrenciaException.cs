namespace Locadora.Domain.Excecoes;

public class ConflitoConcorrenciaException()
    : Exception("O registro foi alterado por outro funcionário. Atualize a tela e tente novamente.");
