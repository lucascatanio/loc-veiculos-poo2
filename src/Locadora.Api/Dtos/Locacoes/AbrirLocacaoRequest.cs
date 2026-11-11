using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Locacoes;

public record AbrirLocacaoRequest(
    int ClienteId,
    int VeiculoId,
    DateTime DataPrevistaDevolucao,
    [MaxLength(500)] string? Observacao,
    List<PagamentoRequest>? Entradas);
