using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Veiculos;

public record AtualizarVeiculoRequest(
    [Required, MaxLength(60)] string Marca,
    [Required, MaxLength(60)] string Modelo,
    [Range(1950, 2100)] int Ano,
    [Required, MaxLength(30)] string Cor,
    int CategoriaId);
