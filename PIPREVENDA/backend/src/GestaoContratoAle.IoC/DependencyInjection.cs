using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalAle.Application;
using PortalAle.Data.SqlServer;

namespace PortalAle.IoC;

/// <summary>
/// Composition root: agrega o registro de dependências de todas as camadas
/// (Application, Data.SqlServer) em um único ponto de entrada, consumido pela Api.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPortalAleServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddSqlServerData(configuration);

        return services;
    }
}
