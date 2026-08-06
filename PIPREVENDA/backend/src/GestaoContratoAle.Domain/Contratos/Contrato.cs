using PortalAle.Domain.Base;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Exceptions;

namespace PortalAle.Domain.Contratos;

/// <summary>
/// Base contratual consolidada (Rede/GRR/B2B). Aggregate Root — ver DT-018/DT-022.
/// Guarda-chuva completo (Fase 1.2 do PLANO-IMPLEMENTACAO.md) está bloqueado por uma
/// pergunta em aberto ao cliente — <see cref="PapelGuardaChuva"/>/<see cref="CodigoGuardaChuva"/>
/// são um valor embutido mínimo para não travar Contrato por causa disso (ver
/// PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 2).
/// </summary>
public class Contrato : Entity
{
    public int ClienteId { get; private set; }

    public virtual Cliente? Cliente { get; private set; }

    public string Pcr { get; private set; } = string.Empty;

    public Segmento Segmento { get; private set; }

    public TipoContrato Tipo { get; private set; }

    public SituacaoMes SituacaoMes { get; private set; }

    public string Bandeira { get; private set; } = string.Empty;

    public bool RegistradoAle { get; private set; }

    public DateOnly? DataRegistroAnp { get; private set; }

    public DateOnly InicioVigencia { get; private set; }

    public DateOnly FimVigencia { get; private set; }

    public decimal VolumeMensalContratado { get; private set; }

    public decimal GalonagemFaturada { get; private set; }

    public decimal MargemBase { get; private set; }

    public decimal? TirContratada { get; private set; }

    public bool Greenfield { get; private set; }

    public bool Denuncia { get; private set; }

    public bool Garantia { get; private set; }

    public bool Sublocado { get; private set; }

    public bool Encerrado { get; private set; }

    public PapelGuardaChuva? PapelGuardaChuva { get; private set; }

    public string? CodigoGuardaChuva { get; private set; }

    public string Diretoria { get; private set; } = string.Empty;

    public string RegionalVendas { get; private set; } = string.Empty;

    public string PontoVenda { get; private set; } = string.Empty;

    public string Consultor { get; private set; } = string.Empty;

    public string? Observacao { get; private set; }

    public string? Clausula { get; private set; }

    public string? CnpjSucedido { get; private set; }

    public string? RazaoSucedido { get; private set; }

    public DateTimeOffset DataCriacao { get; private set; }

    public DateTimeOffset? DataAlteracao { get; private set; }

    private readonly List<EventoContrato> _eventos = [];

    public virtual IReadOnlyCollection<EventoContrato> Eventos => _eventos;

    public Contrato()
    {
    }

    public Contrato(int id)
        : base(id)
    {
    }

    public Contrato(
        int clienteId,
        string pcr,
        Segmento segmento,
        TipoContrato tipo,
        SituacaoMes situacaoMes,
        string bandeira,
        bool registradoAle,
        DateOnly inicioVigencia,
        DateOnly fimVigencia,
        decimal volumeMensalContratado,
        decimal margemBase,
        string diretoria,
        string regionalVendas,
        string pontoVenda,
        string consultor,
        bool greenfield = false)
    {
        ValidarClienteId(clienteId);
        ValidarPcr(pcr);
        ValidarVigencia(inicioVigencia, fimVigencia);
        ValidarVolume(volumeMensalContratado);
        ValidarMargem(margemBase);
        ValidarHierarquiaComercial(diretoria, regionalVendas, pontoVenda, consultor);

        ClienteId = clienteId;
        Pcr = pcr.Trim();
        Segmento = segmento;
        Tipo = tipo;
        SituacaoMes = situacaoMes;
        Bandeira = bandeira.Trim();
        RegistradoAle = registradoAle;
        InicioVigencia = inicioVigencia;
        FimVigencia = fimVigencia;
        VolumeMensalContratado = volumeMensalContratado;
        MargemBase = margemBase;
        Diretoria = diretoria.Trim();
        RegionalVendas = regionalVendas.Trim();
        PontoVenda = pontoVenda.Trim();
        Consultor = consultor.Trim();
        Greenfield = greenfield;
        GalonagemFaturada = 0;
        DataCriacao = DateTimeOffset.UtcNow;

        RegistrarEvento(TipoEventoContrato.NovoNegocio, DateOnly.FromDateTime(DateTime.UtcNow), "Assinatura do contrato.");
    }

    /// <summary>
    /// Regra de negócio fechada (contexto seção 9). Nunca persistir o resultado —
    /// recalcular sempre contra a data de referência (normalmente "hoje").
    /// </summary>
    public (SituacaoPcr Situacao, decimal GalonagemContratada, decimal Saldo, decimal PercentualCumprido) CalcularGalonagem(DateOnly referencia)
    {
        int mesesVigencia = Math.Max(1, MesesEntre(InicioVigencia, FimVigencia));
        decimal galonagemContratada = VolumeMensalContratado * mesesVigencia;
        decimal saldo = Math.Max(0, galonagemContratada - GalonagemFaturada);
        decimal percentual = galonagemContratada == 0 ? 0 : GalonagemFaturada / galonagemContratada;

        SituacaoPcr situacao = GalonagemCalculo.CalcularSituacao(galonagemContratada, GalonagemFaturada, FimVigencia, referencia);

        return (situacao, galonagemContratada, saldo, percentual);
    }

    public void RegistrarEvento(TipoEventoContrato tipo, DateOnly data, string descricao)
    {
        if (Encerrado)
            throw new DomainException("Não é possível registrar evento em contrato encerrado.");

        _eventos.Add(new EventoContrato(Id, tipo, data, descricao));

        if (tipo == TipoEventoContrato.Encerramento)
            Encerrado = true;

        if (tipo == TipoEventoContrato.Denuncia)
            Denuncia = true;

        DataAlteracao = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Atualizado pela integração SAP/PCR (Fase 5, bloqueada) — até lá todo contrato
    /// permanece com GalonagemFaturada = 0 (ver PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 2).
    /// </summary>
    public void AtualizarGalonagemFaturada(decimal novaGalonagemFaturada)
    {
        if (novaGalonagemFaturada < 0)
            throw new DomainException("GalonagemFaturada: não pode ser negativa.");

        GalonagemFaturada = novaGalonagemFaturada;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void DefinirTirContratada(decimal tirContratada)
    {
        TirContratada = tirContratada;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void DefinirRegistroAnp(DateOnly data)
    {
        DataRegistroAnp = data;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void MarcarGarantia(bool possuiGarantia)
    {
        Garantia = possuiGarantia;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void MarcarSublocado(bool sublocado)
    {
        Sublocado = sublocado;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void DefinirGuardaChuva(PapelGuardaChuva papel, string codigoGuardaChuva)
    {
        if (string.IsNullOrWhiteSpace(codigoGuardaChuva))
            throw new DomainException("CodigoGuardaChuva: O campo Código do Guarda-Chuva é obrigatório");

        PapelGuardaChuva = papel;
        CodigoGuardaChuva = codigoGuardaChuva.Trim();
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void DefinirSucessao(string cnpjSucedido, string razaoSucedido)
    {
        if (string.IsNullOrWhiteSpace(cnpjSucedido))
            throw new DomainException("CnpjSucedido: O campo CNPJ Sucedido é obrigatório");

        if (string.IsNullOrWhiteSpace(razaoSucedido))
            throw new DomainException("RazaoSucedido: O campo Razão Social Sucedida é obrigatório");

        CnpjSucedido = cnpjSucedido.Trim();
        RazaoSucedido = razaoSucedido.Trim();
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void DefinirObservacaoEClausula(string? observacao, string? clausula)
    {
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
        Clausula = string.IsNullOrWhiteSpace(clausula) ? null : clausula.Trim();
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    private static void ValidarClienteId(int clienteId)
    {
        if (clienteId <= 0)
            throw new DomainException("ClienteId: O campo Cliente é obrigatório");
    }

    private static void ValidarPcr(string pcr)
    {
        if (string.IsNullOrWhiteSpace(pcr))
            throw new DomainException("Pcr: O campo PCR/PCF é obrigatório");

        if (pcr.Trim().Length > 20)
            throw new DomainException("Pcr: O campo PCR/PCF deve ter no máximo 20 caracteres");
    }

    private static void ValidarVigencia(DateOnly inicio, DateOnly fim)
    {
        if (fim < inicio)
            throw new DomainException("FimVigencia: A data de fim deve ser posterior ao início");
    }

    private static void ValidarVolume(decimal volume)
    {
        if (volume <= 0)
            throw new DomainException("VolumeMensalContratado: Deve ser maior que zero");
    }

    private static void ValidarMargem(decimal margem)
    {
        if (margem < 0)
            throw new DomainException("MargemBase: Não pode ser negativa");
    }

    private static void ValidarHierarquiaComercial(string diretoria, string regionalVendas, string pontoVenda, string consultor)
    {
        if (string.IsNullOrWhiteSpace(diretoria))
            throw new DomainException("Diretoria: O campo Diretoria é obrigatório");

        if (string.IsNullOrWhiteSpace(regionalVendas))
            throw new DomainException("RegionalVendas: O campo Regional de Vendas é obrigatório");

        if (string.IsNullOrWhiteSpace(pontoVenda))
            throw new DomainException("PontoVenda: O campo Ponto de Venda é obrigatório");

        if (string.IsNullOrWhiteSpace(consultor))
            throw new DomainException("Consultor: O campo Consultor é obrigatório");
    }

    private static int MesesEntre(DateOnly a, DateOnly b)
        => ((b.Year - a.Year) * 12) + (b.Month - a.Month);
}
