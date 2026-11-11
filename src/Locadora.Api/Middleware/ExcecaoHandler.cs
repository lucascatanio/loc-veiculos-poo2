using Locadora.Domain.Excecoes;
using Microsoft.AspNetCore.Diagnostics;

namespace Locadora.Api.Middleware;

public class ExcecaoHandler(IProblemDetailsService problemDetails, ILogger<ExcecaoHandler> logger) : IExceptionHandler
{
    private static readonly Dictionary<Type, (int Status, string Titulo)> Mapa = new()
    {
        [typeof(DomainException)] = (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada"),
        [typeof(NaoEncontradoException)] = (StatusCodes.Status404NotFound, "Não encontrado"),
        [typeof(ConflitoConcorrenciaException)] = (StatusCodes.Status409Conflict, "Conflito de concorrência"),
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        var (status, titulo, detalhe) = Classificar(exception);

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = { Status = status, Title = titulo, Detail = detalhe },
        });
    }

    private (int Status, string Titulo, string Detalhe) Classificar(Exception exception)
    {
        // Percorre a hierarquia para que subclasses de DomainException também caiam em 422.
        for (var tipo = exception.GetType(); tipo is not null; tipo = tipo.BaseType)
        {
            if (Mapa.TryGetValue(tipo, out var mapeado))
                return (mapeado.Status, mapeado.Titulo, exception.Message);
        }

        logger.LogError(exception, "Erro não tratado");
        return (StatusCodes.Status500InternalServerError, "Erro inesperado", "Ocorreu um erro inesperado. Tente novamente.");
    }
}
