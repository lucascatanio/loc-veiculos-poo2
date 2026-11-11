namespace Locadora.Api.Dtos.Locacoes;

public record PagamentoResponse(int Id, decimal Valor, DateTime DataPagamento, string Tipo, string Forma);
