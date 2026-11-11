using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Locacoes;

public record PagamentoRequest(
    [Range(0.01, 99999999.99)] decimal Valor,
    [Required] string Forma);
