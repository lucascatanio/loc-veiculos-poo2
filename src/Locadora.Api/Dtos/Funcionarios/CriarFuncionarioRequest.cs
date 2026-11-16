using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Funcionarios;

public record CriarFuncionarioRequest(
    [Required(ErrorMessage = "Informe o nome."), MaxLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")] string Nome,
    [Required(ErrorMessage = "Informe o e-mail."), EmailAddress(ErrorMessage = "E-mail inválido."), MaxLength(150, ErrorMessage = "O e-mail pode ter no máximo 150 caracteres.")] string Email,
    [Required(ErrorMessage = "Informe a senha."), MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres."), MaxLength(72, ErrorMessage = "A senha pode ter no máximo 72 caracteres.")] string Senha);
