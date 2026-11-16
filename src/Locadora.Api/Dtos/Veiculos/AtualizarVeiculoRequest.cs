using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Veiculos;

public record AtualizarVeiculoRequest(
    [Required(ErrorMessage = "Informe a marca."), MaxLength(60, ErrorMessage = "A marca pode ter no máximo 60 caracteres.")] string Marca,
    [Required(ErrorMessage = "Informe o modelo."), MaxLength(60, ErrorMessage = "O modelo pode ter no máximo 60 caracteres.")] string Modelo,
    [Range(1950, 2100, ErrorMessage = "O ano deve estar entre 1950 e 2100.")] int Ano,
    [Required(ErrorMessage = "Informe a cor."), MaxLength(30, ErrorMessage = "A cor pode ter no máximo 30 caracteres.")] string Cor,
    int CategoriaId);
