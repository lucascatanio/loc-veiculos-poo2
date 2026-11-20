using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Categorias;

public record CriarCategoriaRequest(
    [Required(ErrorMessage = "Informe o nome."), MaxLength(80, ErrorMessage = "O nome pode ter no máximo 80 caracteres.")] string Nome,
    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true, ErrorMessage = "A diária deve estar entre R$ 0,01 e R$ 99.999.999,99.")] decimal ValorDiaria);
