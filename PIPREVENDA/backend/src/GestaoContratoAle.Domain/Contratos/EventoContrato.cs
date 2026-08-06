using PortalAle.Domain.Base;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Domain.Contratos;

/// <summary>
/// Evento do histórico de ciclo de vida do contrato. Entidade filha de <see cref="Contrato"/> —
/// sem repositório próprio (DT-018, "Repository APENAS para Aggregate Roots"). Só pode ser
/// criada via <see cref="Contrato.RegistrarEvento"/> (construtor internal).
/// </summary>
public class EventoContrato : Entity
{
    public int ContratoId { get; private set; }

    public virtual Contrato? Contrato { get; private set; }

    public TipoEventoContrato Tipo { get; private set; }

    public DateOnly Data { get; private set; }

    public string Descricao { get; private set; } = string.Empty;

    public EventoContrato()
    {
    }

    public EventoContrato(int id)
        : base(id)
    {
    }

    internal EventoContrato(int contratoId, TipoEventoContrato tipo, DateOnly data, string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descricao: O campo Descrição é obrigatório");

        if (descricao.Trim().Length > 500)
            throw new DomainException("Descricao: O campo Descrição deve ter no máximo 500 caracteres");

        ContratoId = contratoId;
        Tipo = tipo;
        Data = data;
        Descricao = descricao.Trim();
    }
}
