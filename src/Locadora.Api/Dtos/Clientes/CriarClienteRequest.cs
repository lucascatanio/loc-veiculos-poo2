using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Clientes;

public record CriarClienteRequest(
    [Required(ErrorMessage = "Informe o nome."), MaxLength(120, ErrorMessage = "O nome pode ter no máximo 120 caracteres.")] string Nome,
    [Required(ErrorMessage = "Informe o CPF."), MaxLength(14, ErrorMessage = "O CPF pode ter no máximo 14 caracteres.")] string Cpf,
    [Required(ErrorMessage = "Informe a CNH."), MaxLength(20, ErrorMessage = "A CNH pode ter no máximo 20 caracteres.")] string Cnh,
    [Required(ErrorMessage = "Informe o telefone."), MaxLength(20, ErrorMessage = "O telefone pode ter no máximo 20 caracteres.")] string Telefone,
    [RegularExpression(EmailOpcional.Padrao, ErrorMessage = "E-mail inválido."), MaxLength(150, ErrorMessage = "O e-mail pode ter no máximo 150 caracteres.")] string? Email);
