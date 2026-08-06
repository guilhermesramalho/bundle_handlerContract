using PortalAle.Domain.Contratos;

namespace PortalAle.Application.Contratos.RegistrarEvento;

public record RegistrarEventoContratoResponse(
    int ContratoId,
    TipoEventoContrato Tipo,
    DateOnly Data,
    string Descricao,
    bool ContratoEncerrado,
    bool ContratoEmDenuncia);
