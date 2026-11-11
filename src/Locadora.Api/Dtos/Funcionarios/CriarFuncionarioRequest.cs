using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Funcionarios;

public record CriarFuncionarioRequest(
    [Required, MaxLength(120)] string Nome,
    [Required, EmailAddress, MaxLength(150)] string Email,
    [Required, MinLength(6), MaxLength(72)] string Senha);
