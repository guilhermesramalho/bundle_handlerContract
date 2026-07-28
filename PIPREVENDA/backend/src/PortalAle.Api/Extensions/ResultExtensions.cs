using System.Diagnostics;
using PortalAle.Application.Base;

namespace PortalAle.Api.Extensions;

/// <summary>
/// Converte Result/Result&lt;T&gt; da camada de Aplicação em respostas HTTP
/// ProblemDetails (RFC 7807). Ver DT-013.
/// </summary>
public static class ResultExtensions
{
    private const string ErrorTypeBaseUrl = "https://api.portalale.com.br/errors";

    /// <summary>
    /// Converte um Result de falha em ProblemDetails simples (sem agrupamento por campo).
    /// </summary>
    public static IResult ToProblemDetails(
        this Result result,
        int statusCode = StatusCodes.Status400BadRequest,
        string? type = null)
    {
        var errorType = type ?? GetErrorType(statusCode);

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitle(statusCode),
            detail: result.Errors.FirstOrDefault() ?? "Ocorreu um erro ao processar a requisição",
            type: $"{ErrorTypeBaseUrl}/{errorType}",
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = result.Errors,
                ["traceId"] = Activity.Current?.Id,
            });
    }

    /// <summary>
    /// Converte um Result de falha em ValidationProblem, agrupando erros por campo.
    /// Espera mensagens no formato "Campo: Mensagem"; mensagens sem esse formato
    /// são agrupadas na chave "_general".
    /// </summary>
    public static IResult ToValidationProblem(this Result result)
    {
        var errosPorCampo = new Dictionary<string, string[]>();

        foreach (var erro in result.Errors)
        {
            var partes = erro.Split(':', 2, StringSplitOptions.TrimEntries);
            var campo = partes.Length == 2 ? partes[0] : "_general";
            var mensagem = partes.Length == 2 ? partes[1] : erro;

            errosPorCampo[campo] = errosPorCampo.TryGetValue(campo, out var existentes)
                ? [.. existentes, mensagem]
                : [mensagem];
        }

        return Results.ValidationProblem(
            errosPorCampo,
            title: "Erro de Validação",
            detail: "Um ou mais campos estão inválidos",
            type: $"{ErrorTypeBaseUrl}/validation-error",
            statusCode: StatusCodes.Status400BadRequest,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = Activity.Current?.Id,
            });
    }

    private static string GetErrorType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "validation-error",
        StatusCodes.Status404NotFound => "not-found",
        StatusCodes.Status409Conflict => "conflict",
        StatusCodes.Status403Forbidden => "forbidden",
        _ => "error",
    };

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Erro de Validação",
        StatusCodes.Status404NotFound => "Recurso Não Encontrado",
        StatusCodes.Status409Conflict => "Conflito de Estado",
        StatusCodes.Status403Forbidden => "Acesso Negado",
        _ => "Erro",
    };
}
