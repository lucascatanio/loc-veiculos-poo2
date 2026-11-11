using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Clientes;

public record CriarClienteRequest(
    [Required, MaxLength(120)] string Nome,
    [Required, MaxLength(14)] string Cpf,
    [Required, MaxLength(20)] string Cnh,
    [Required, MaxLength(20)] string Telefone,
    [RegularExpression(EmailOpcional.Padrao, ErrorMessage = "E-mail inválido."), MaxLength(150)] string? Email);
