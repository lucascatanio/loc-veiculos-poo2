using Locadora.Api.Dtos.Locacoes;

namespace Locadora.Desktop.ViewModels.Locacoes;

public sealed record FatoDoContrato(string Rotulo, string Texto);

public sealed record TotalDoContrato(string Rotulo, decimal Valor);

public sealed record LinhaDoExtrato(DateTime Data, string Tipo, string Forma, decimal Valor)
{
    public static LinhaDoExtrato De(PagamentoResponse pagamento) => new(
        pagamento.DataPagamento,
        pagamento.Tipo == "Saida" ? "Na devolução" : pagamento.Tipo,
        FormasDePagamento.NomeDe(pagamento.Forma),
        pagamento.Valor);
}
