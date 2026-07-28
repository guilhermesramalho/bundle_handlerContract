using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortalAle.Application.Base;
using PortalAle.Data;
using PortalAle.Domain.Base;

namespace PortalAle.Data.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerData(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PortalAle")
            ?? throw new InvalidOperationException("ConnectionString 'PortalAle' não configurada.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
                sqlServerOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Permite injetar DbContext (abstração) diretamente nos CommandHandlers, conforme DT-020/DT-021.
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var assembly = typeof(DependencyInjection).Assembly;

        // Registro automático de repositórios (I{Entidade}Repository) e query services (I{Entidade}Queries).
        // Ver DT-018 (Passo 4) e DT-019 (seção 5).
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQuery)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        return services;
    }
}
