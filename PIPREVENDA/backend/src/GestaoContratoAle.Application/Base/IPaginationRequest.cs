namespace PortalAle.Application.Base;

/// <summary>
/// Contrato de paginação para requests de listagem. Ver DT-019.
/// </summary>
public interface IPaginationRequest
{
    int PageIndex { get; }

    int PageSize { get; }
}
