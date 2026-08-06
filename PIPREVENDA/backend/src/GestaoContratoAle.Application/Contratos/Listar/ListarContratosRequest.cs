using PortalAle.Application.Base;
using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.Listar;

/// <summary>
/// Request de listagem paginada/ordenável/filtrável de contratos — cobre exatamente os
/// filtros já implementados na tela `/contratos` do frontend (Fase F2, sobre mock) para
/// que a troca de mock por API real (Fase F5 do PLANO-IMPLEMENTACAO-FRONTEND.md) não
/// exija mudar a tela. Ver PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 5.2.
/// </summary>
public record ListarContratosRequest : PaginationRequest, ISortableRequest, IRequest<Result<PaginationResponse<ListarContratosResponse>>>
{
    /// <summary>Busca combinada — razão social, CNPJ, PCR/PCF, SAP, grupo econômico.</summary>
    public string? Busca { get; init; }

    /// <summary>
    /// Array (não List/IReadOnlyList) — o model binding de query string do Minimal API
    /// ([AsParameters]) só infere automaticamente parâmetros multivalorados
    /// (?segmentos=Rede&amp;segmentos=Grr) para tipo array; List{T} é inferido como Body e
    /// quebra o startup ("Body was inferred but the method does not allow inferred body
    /// parameters") — descoberto rodando a API de verdade, não é escolha estética.
    /// </summary>
    public Segmento[]? Segmentos { get; init; }

    public TipoContrato[]? Tipos { get; init; }

    public SituacaoPcr[]? SituacoesPcr { get; init; }

    public bool? EmDenuncia { get; init; }

    public string? Bandeira { get; init; }

    public string[]? Ufs { get; init; }

    public string? Diretoria { get; init; }

    public string? RegionalVendas { get; init; }

    public string? PontoVenda { get; init; }

    public string? Consultor { get; init; }

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }
}
