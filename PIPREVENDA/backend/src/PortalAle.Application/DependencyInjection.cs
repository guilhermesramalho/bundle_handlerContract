using Microsoft.Extensions.DependencyInjection;
using PortalAle.Application.Base;

namespace PortalAle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<,>),
                typeof(ICommandHandler<>),
                typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // TryDecorate (em vez de Decorate) porque este é um esqueleto sem CommandHandlers
        // ainda implementados: Decorate lançaria exceção por não encontrar nenhum registro
        // do tipo aberto para decorar. Uma vez criados os primeiros Commands, o decorator
        // passa a interceptá-los normalmente.
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(TransactionCommandHandlerDecorator<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(TransactionCommandHandlerDecorator<>));

        return services;
    }
}
