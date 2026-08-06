using PortalAle.Application.Base;
using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.Criar;

/// <summary>
/// Request de escrita para criar um novo contrato.
/// </summary>
public record CriarContratoRequest(
    int ClienteId,
    string Pcr,
    Segmento Segmento,
    TipoContrato Tipo,
    SituacaoMes SituacaoMes,
    string Bandeira,
    bool RegistradoAle,
    DateOnly InicioVigencia,
    DateOnly FimVigencia,
    decimal VolumeMensalContratado,
    decimal MargemBase,
    string Diretoria,
    string RegionalVendas,
    string PontoVenda,
    string Consultor,
    bool Greenfield = false) : IRequest<Result<CriarContratoResponse>>;
