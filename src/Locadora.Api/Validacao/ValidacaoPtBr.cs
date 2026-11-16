using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Locadora.Api.Validacao;

public static class ValidacaoPtBr
{
    public static void ConfigurarMensagensEmPortugues(this MvcOptions options)
    {
        // Sem isso o parâmetro `request` (não anulável) gera um "The request field is required." extra.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

        var mensagens = options.ModelBindingMessageProvider;

        mensagens.SetValueMustNotBeNullAccessor(_ => "Informe um valor.");
        mensagens.SetMissingBindRequiredValueAccessor(campo => $"Informe o campo {campo}.");
        mensagens.SetMissingKeyOrValueAccessor(() => "Informe um valor.");
        mensagens.SetMissingRequestBodyRequiredValueAccessor(() => "Envie os dados no corpo da requisição.");
        mensagens.SetAttemptedValueIsInvalidAccessor((valor, campo) => $"O valor '{valor}' não é válido para {campo}.");
        mensagens.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        mensagens.SetUnknownValueIsInvalidAccessor(campo => $"O valor informado não é válido para {campo}.");
        mensagens.SetNonPropertyUnknownValueIsInvalidAccessor(() => "O valor informado não é válido.");
        mensagens.SetValueIsInvalidAccessor(valor => $"O valor '{valor}' não é válido.");
        mensagens.SetValueMustBeANumberAccessor(campo => $"O campo {campo} deve ser um número.");
        mensagens.SetNonPropertyValueMustBeANumberAccessor(() => "O valor deve ser um número.");
    }

    // O JsonInputFormatter registra a exceção do System.Text.Json, cuja mensagem vem em inglês e cita tipos internos.
    public static IActionResult CriarRespostaInvalida(ActionContext contexto)
    {
        foreach (var entrada in contexto.ModelState.Values)
        {
            for (var i = 0; i < entrada.Errors.Count; i++)
            {
                if (entrada.Errors[i].Exception is not null)
                    entrada.Errors[i] = new ModelError("Valor inválido ou JSON malformado.");
            }
        }

        var problema = new ValidationProblemDetails(contexto.ModelState)
        {
            Title = "Dados inválidos.",
            Status = StatusCodes.Status400BadRequest,
        };

        return new BadRequestObjectResult(problema) { ContentTypes = { "application/problem+json" } };
    }
}
