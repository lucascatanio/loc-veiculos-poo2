namespace Locadora.Api.Dtos.Clientes;

internal static class EmailOpcional
{
    // [EmailAddress] rejeita string vazia; aqui vazio é aceito e vira null na entidade.
    public const string Padrao = @"^\s*$|^\s*[^@\s]+@[^@\s]+\.[^@\s]+\s*$";
}
