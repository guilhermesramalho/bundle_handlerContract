using PortalAle.Domain.Base;
using PortalAle.Domain.Exceptions;
using PortalAle.Domain.GruposEconomicos;

namespace PortalAle.Domain.Clientes;

/// <summary>
/// Cadastro de cliente (pessoa jurídica). Aggregate Root — ver DT-018/DT-022.
/// </summary>
public class Cliente : Entity
{
    public string Nome { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string Endereco { get; private set; } = string.Empty;

    public bool Ativo { get; private set; }

    /// <summary>Número SAP do cliente (10 dígitos) — identifica o cliente, distinto de PCR/PCF (contexto seção 9). Nullable: nem todo cliente tem SAP ainda (integração bloqueada, Fase 5).</summary>
    public string? NrSap { get; private set; }

    /// <summary>UF do CNPJ/ponto de venda. Nullable: endereço hoje é texto livre, preenchimento estruturado é incremental.</summary>
    public string? Uf { get; private set; }

    public int? GrupoEconomicoId { get; private set; }

    public virtual GrupoEconomico? GrupoEconomico { get; private set; }

    public DateTimeOffset DataCriacao { get; private set; }

    public DateTimeOffset? DataAlteracao { get; private set; }

    public DateTimeOffset? DataExclusao { get; private set; }

    public Cliente()
    {
    }

    public Cliente(int id)
        : base(id)
    {
    }

    public Cliente(string nome, string cnpj, string endereco)
    {
        ValidarNome(nome);
        ValidarCnpj(cnpj);
        ValidarEndereco(endereco);

        Nome = nome.Trim();
        Cnpj = SomenteDigitos(cnpj);
        Endereco = endereco.Trim();
        Ativo = true;
        DataCriacao = DateTimeOffset.UtcNow;
    }

    public void Atualizar(string nome, string cnpj, string endereco, bool ativo)
    {
        GarantirNaoExcluido();

        ValidarNome(nome);
        ValidarCnpj(cnpj);
        ValidarEndereco(endereco);

        Nome = nome.Trim();
        Cnpj = SomenteDigitos(cnpj);
        Endereco = endereco.Trim();
        Ativo = ativo;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Preenche/atualiza os dados complementares do cliente (SAP, UF, grupo econômico) —
    /// mutação separada do construtor rico porque, diferente de Nome/Cnpj/Endereco,
    /// estes campos nascem vazios (não fazem parte da criação original de Cliente) e são
    /// preenchidos progressivamente pelo cadastro manual de GrupoEconomico (Fase 1.1) e,
    /// no futuro, pela integração SAP (Fase 5).
    /// </summary>
    public void AtribuirDadosComplementares(string? nrSap, string? uf, int? grupoEconomicoId)
    {
        GarantirNaoExcluido();

        if (!string.IsNullOrWhiteSpace(nrSap))
            ValidarNrSap(nrSap);

        if (!string.IsNullOrWhiteSpace(uf))
            ValidarUf(uf);

        NrSap = string.IsNullOrWhiteSpace(nrSap) ? null : SomenteDigitos(nrSap);
        Uf = string.IsNullOrWhiteSpace(uf) ? null : uf.Trim().ToUpperInvariant();
        GrupoEconomicoId = grupoEconomicoId;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    private static void ValidarNrSap(string nrSap)
    {
        if (SomenteDigitos(nrSap).Length != 10)
            throw new DomainException("NrSap: O número SAP do cliente deve ter 10 dígitos");
    }

    private static void ValidarUf(string uf)
    {
        if (uf.Trim().Length != 2)
            throw new DomainException("Uf: A UF deve ter 2 caracteres");
    }

    public void ExcluirLogicamente()
    {
        GarantirNaoExcluido();

        Ativo = false;
        DataExclusao = DateTimeOffset.UtcNow;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    private void GarantirNaoExcluido()
    {
        if (DataExclusao is not null)
            throw new DomainException("Cliente já foi excluído.");
    }

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome: O campo Nome é obrigatório");

        if (nome.Trim().Length > 200)
            throw new DomainException("Nome: O campo Nome deve ter no máximo 200 caracteres");
    }

    private static void ValidarEndereco(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new DomainException("Endereco: O campo Endereço é obrigatório");

        if (endereco.Trim().Length > 200)
            throw new DomainException("Endereco: O campo Endereço deve ter no máximo 200 caracteres");
    }

    private static void ValidarCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            throw new DomainException("Cnpj: O campo CNPJ é obrigatório");

        if (!EhCnpjValido(cnpj))
            throw new DomainException("Cnpj: CNPJ inválido");
    }

    private static string SomenteDigitos(string valor) => new([.. valor.Where(char.IsDigit)]);

    private static bool EhCnpjValido(string cnpj)
    {
        string digitos = SomenteDigitos(cnpj);

        if (digitos.Length != 14)
            return false;

        if (digitos.Distinct().Count() == 1)
            return false;

        int[] multiplicadoresDigito1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicadoresDigito2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        string base12 = digitos[..12];
        int digito1 = CalcularDigitoVerificador(base12, multiplicadoresDigito1);

        string base13 = base12 + digito1;
        int digito2 = CalcularDigitoVerificador(base13, multiplicadoresDigito2);

        return digitos == base13 + digito2;
    }

    private static int CalcularDigitoVerificador(string baseNumerica, int[] multiplicadores)
    {
        int soma = 0;

        for (int i = 0; i < multiplicadores.Length; i++)
            soma += (baseNumerica[i] - '0') * multiplicadores[i];

        int resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }
}
