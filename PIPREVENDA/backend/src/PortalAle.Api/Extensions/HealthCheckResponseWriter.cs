using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PortalAle.Api.Extensions;

/// <summary>
/// Formata a resposta do endpoint /health como JSON, incluindo o status individual
/// de cada verificação (ex.: conectividade com o SQL Server).
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions s_serializerOptions = new() { WriteIndented = true };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, s_serializerOptions));
    }
}
