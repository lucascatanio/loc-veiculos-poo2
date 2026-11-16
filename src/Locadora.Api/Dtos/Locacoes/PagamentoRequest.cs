using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Locacoes;

// Valor e Forma sem validação de conteúdo aqui: as regras ficam no Pagamento (422), não no formato (400).
public record PagamentoRequest(
    decimal Valor,
    [Required] string Forma);
