using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha);
