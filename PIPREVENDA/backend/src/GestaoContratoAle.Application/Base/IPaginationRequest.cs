namespace PortalAle.Application.Base;

/// <summary>
/// Contrato de paginação para requests de listagem. Ver DT-019.
/// Nullable (não int puro): o Minimal API ([AsParameters]) trata propriedade `int` não-anulável
/// como parâmetro obrigatório de query string, ignorando o valor padrão do record — descoberto
/// rodando a API de verdade (GET sem `?pageIndex=` quebrava com 500 "Required parameter
/// \"int PageIndex\" was not provided"). Os valores efetivos, sempre normalizados, vêm de
/// NormalizedPageIndex/NormalizedPageSize (ver PaginationRequest).
/// </summary>
public interface IPaginationRequest
{
    int? PageIndex { get; }

    int? PageSize { get; }
}
