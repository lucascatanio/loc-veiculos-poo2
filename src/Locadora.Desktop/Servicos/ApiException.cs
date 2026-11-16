using System.Net.Http;
using System.Text.Json;

namespace Locadora.Desktop.Servicos;

public class ApiException(int status, string detalhe, IReadOnlyList<string> erros) : Exception(detalhe)
{
    // 0 quando a API nem respondeu (conexão recusada, timeout).
    public int Status { get; } = status;
    public string Detalhe { get; } = detalhe;
    public IReadOnlyList<string> Erros { get; } = erros;

    public static ApiException SemConexao(Uri? urlBase) =>
        new(0, $"Não foi possível falar com a API em {urlBase}. Ela está rodando?", []);

    public static async Task<ApiException> CriarAsync(HttpResponseMessage resposta, CancellationToken ct = default)
    {
        var status = (int)resposta.StatusCode;
        var corpo = await resposta.Content.ReadAsStringAsync(ct);
        var erros = new List<string>();
        string? detalhe = null;

        try
        {
            using var doc = JsonDocument.Parse(corpo);
            var raiz = doc.RootElement;
            if (raiz.ValueKind == JsonValueKind.Object)
            {
                if (raiz.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                    foreach (var campo in errors.EnumerateObject())
                        foreach (var mensagem in campo.Value.EnumerateArray())
                            erros.Add(mensagem.GetString() ?? "");

                detalhe = TextoDe(raiz, "detail") ?? TextoDe(raiz, "title");
            }
        }
        catch (JsonException)
        {
            // corpo vazio ou que não é JSON (ex.: 401 sem corpo): cai na mensagem padrão do status
        }

        var texto = erros.Count > 0 ? string.Join(Environment.NewLine, erros) : detalhe ?? MensagemPadrao(status);
        return new ApiException(status, texto, erros);
    }

    private static string? TextoDe(JsonElement objeto, string propriedade) =>
        objeto.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String ? valor.GetString() : null;

    private static string MensagemPadrao(int status) => status switch
    {
        401 => "Sessão expirada ou não autorizada. Entre novamente.",
        403 => "Você não tem permissão para essa operação.",
        404 => "Registro não encontrado.",
        >= 500 => "A API encontrou um erro interno.",
        _ => $"A API respondeu com erro {status}."
    };
}
