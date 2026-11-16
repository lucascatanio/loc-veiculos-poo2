using System.ComponentModel.DataAnnotations;

namespace Locadora.Api.Dtos.Locacoes;

public record AbrirLocacaoRequest(
    int ClienteId,
    int VeiculoId,
    DateTime DataPrevistaDevolucao,
    [MaxLength(500, ErrorMessage = "A observação pode ter no máximo 500 caracteres.")] string? Observacao,
    List<PagamentoRequest>? Entradas);
