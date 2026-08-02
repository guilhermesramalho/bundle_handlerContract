namespace PortalAle.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma invariante de domínio é violada. Entidades devem
/// lançá-la imediatamente ao detectar dados inválidos, antes de qualquer atribuição.
/// Ver DT-022.
/// </summary>
public class DomainException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public DomainException(string message)
        : base(message)
    {
        Errors = [message];
    }

    public DomainException(List<string> errors)
        : base(string.Join(" ", errors))
    {
        Errors = errors;
    }
}
