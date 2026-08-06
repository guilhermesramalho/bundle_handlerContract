using PortalAle.Domain.Base;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Domain.GruposEconomicos;

/// <summary>
/// Grupo econômico (conjunto de CNPJs do mesmo grupo controlador). Aggregate Root — ver DT-018/DT-022.
/// Cadastro manual por ora — no desenho-alvo viria do SAP (integração bloqueada, Fase 5 do
/// PLANO-IMPLEMENTACAO.md) — placeholder consciente, ver PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 1.
/// </summary>
public class GrupoEconomico : Entity
{
    public string Codigo { get; private set; } = string.Empty;

    public string Nome { get; private set; } = string.Empty;

    public DateTimeOffset DataCriacao { get; private set; }

    public DateTimeOffset? DataAlteracao { get; private set; }

    public GrupoEconomico()
    {
    }

    public GrupoEconomico(int id)
        : base(id)
    {
    }

    public GrupoEconomico(string codigo, string nome)
    {
        ValidarCodigo(codigo);
        ValidarNome(nome);

        Codigo = codigo.Trim();
        Nome = nome.Trim();
        DataCriacao = DateTimeOffset.UtcNow;
    }

    private static void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("Codigo: O campo Código é obrigatório");

        if (codigo.Trim().Length != 3 || !codigo.Trim().All(char.IsDigit))
            throw new DomainException("Codigo: O campo Código deve ter exatamente 3 dígitos (código SAP)");
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome: O campo Nome é obrigatório");

        if (nome.Trim().Length > 200)
            throw new DomainException("Nome: O campo Nome deve ter no máximo 200 caracteres");
    }
}
