using Locadora.Domain.Enums;

namespace Locadora.Application.AppServices;

public record EntradaPagamento(decimal Valor, FormaPagamento Forma);
