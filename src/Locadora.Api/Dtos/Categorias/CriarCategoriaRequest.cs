using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Categorias;

public record CriarCategoriaRequest(
    [Required(ErrorMessage = "Informe o nome."), MaxLength(80, ErrorMessage = "O nome pode ter no máximo 80 caracteres.")] string Nome,
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "A diária deve ser maior que zero.")] decimal ValorDiaria);
