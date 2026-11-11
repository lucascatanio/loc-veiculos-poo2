namespace Locadora.Api.Dtos.Clientes;

public record ClienteResponse(int Id, string Nome, string Cpf, string Cnh, string Telefone, string? Email, bool Ativo);
