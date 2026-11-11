using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Funcionarios;

public record AtualizarFuncionarioRequest(
    [Required, MaxLength(120)] string Nome,
    [Required, EmailAddress, MaxLength(150)] string Email);
