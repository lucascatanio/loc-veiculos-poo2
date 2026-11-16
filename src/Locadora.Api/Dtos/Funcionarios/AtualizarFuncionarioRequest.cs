using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Funcionarios;

public record AtualizarFuncionarioRequest(
    [Required(ErrorMessage = "Informe o nome."), MaxLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")] string Nome,
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido."), MaxLength(150, ErrorMessage = "O e-mail pode ter no máximo 150 caracteres.")] string Email);
