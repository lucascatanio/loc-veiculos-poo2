namespace Locadora.Api.Dtos.Auth;

public record LoginResponse(string Token, string Nome, DateTime ExpiraEm);
