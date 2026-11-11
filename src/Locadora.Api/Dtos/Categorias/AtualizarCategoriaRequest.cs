using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Categorias;

public record AtualizarCategoriaRequest(
    [Required, MaxLength(80)] string Nome,
    [Range(typeof(decimal), "0.01", "99999999.99")] decimal ValorDiaria);
