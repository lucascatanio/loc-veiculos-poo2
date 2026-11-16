using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido.")] string Email,
    [Required(ErrorMessage = "Informe a senha.")] string Senha);
