using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Locacoes;

public record FinalizarLocacaoRequest(
    int KmDevolucao,
    [MaxLength(500)] string? Observacao,
    List<PagamentoRequest>? Saidas);
