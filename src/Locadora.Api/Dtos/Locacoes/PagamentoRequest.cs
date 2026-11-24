using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Locacoes;

public record PagamentoRequest(
    decimal Valor,
    [Required(ErrorMessage = "Informe a forma de pagamento.")] string Forma);
