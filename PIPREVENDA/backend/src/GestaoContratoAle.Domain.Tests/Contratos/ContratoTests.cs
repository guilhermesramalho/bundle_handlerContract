using PortalAle.Domain.Contratos;
using PortalAle.Domain.Exceptions;

namespace GestaoContratoAle.Domain.Tests.Contratos;

[TestClass]
public class ContratoTests
{
    private static Contrato CriarContratoValido(
        DateOnly? inicio = null,
        DateOnly? fim = null,
        decimal volumeMensal = 400m)
    {
        return new Contrato(
            clienteId: 1,
            pcr: "700.123",
            segmento: Segmento.Rede,
            tipo: TipoContrato.Pcvm,
            situacaoMes: SituacaoMes.Ativo,
            bandeira: "Bandeira ALE",
            registradoAle: true,
            inicioVigencia: inicio ?? new DateOnly(2024, 1, 1),
            fimVigencia: fim ?? new DateOnly(2026, 1, 1),
            volumeMensalContratado: volumeMensal,
            margemBase: 0.45m,
            diretoria: "Sudeste",
            regionalVendas: "GR São Paulo Interior",
            pontoVenda: "RN Campinas",
            consultor: "Marcos Vidal");
    }

    [TestMethod]
    public void Criar_ComPcrVazio_DeveLancarDomainException()
    {
        // ============ ARRANGE / ACT ============
        void Acao() => new Contrato(
            1, "", Segmento.Rede, TipoContrato.Pcvm, SituacaoMes.Ativo, "Bandeira ALE", true,
            new DateOnly(2024, 1, 1), new DateOnly(2026, 1, 1), 400m, 0.45m,
            "Sudeste", "GR São Paulo Interior", "RN Campinas", "Marcos Vidal");

        // ============ ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(Acao);
        StringAssert.Contains(ex.Message, "PCR/PCF é obrigatório");
    }

    [TestMethod]
    public void Criar_ComFimVigenciaAnteriorAoInicio_DeveLancarDomainException()
    {
        // ============ ARRANGE / ACT ============
        void Acao() => CriarContratoValido(inicio: new DateOnly(2026, 1, 1), fim: new DateOnly(2024, 1, 1));

        // ============ ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(Acao);
        StringAssert.Contains(ex.Message, "posterior ao início");
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-100)]
    public void Criar_ComVolumeMensalZeroOuNegativo_DeveLancarDomainException(int volume)
    {
        // ============ ARRANGE / ACT ============
        void Acao() => CriarContratoValido(volumeMensal: volume);

        // ============ ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(Acao);
        StringAssert.Contains(ex.Message, "VolumeMensalContratado");
    }

    [TestMethod]
    public void Criar_ComDadosValidos_DeveRegistrarEventoNovoNegocioAutomaticamente()
    {
        // ============ ARRANGE / ACT ============
        var contrato = CriarContratoValido();

        // ============ ASSERT ============
        Assert.AreEqual(1, contrato.Eventos.Count);
        Assert.AreEqual(TipoEventoContrato.NovoNegocio, contrato.Eventos.Single().Tipo);
        Assert.IsFalse(contrato.Encerrado);
        Assert.AreEqual(0m, contrato.GalonagemFaturada);
    }

    [TestMethod]
    public void CalcularGalonagem_ComFaturamentoAbaixoDoContratadoEPrazoVigente_DeveRetornarVigente()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido(inicio: new DateOnly(2024, 1, 1), fim: new DateOnly(2026, 1, 1), volumeMensal: 100m);
        contrato.AtualizarGalonagemFaturada(500m); // bem abaixo dos 24 meses * 100 = 2400 contratados

        // ============ ACT ============
        var (situacao, galonagemContratada, saldo, percentual) = contrato.CalcularGalonagem(new DateOnly(2025, 1, 1));

        // ============ ASSERT ============
        Assert.AreEqual(SituacaoPcr.Vigente, situacao);
        Assert.AreEqual(2400m, galonagemContratada);
        Assert.AreEqual(1900m, saldo);
        Assert.IsTrue(percentual < 1);
    }

    [TestMethod]
    public void CalcularGalonagem_ComFaturamentoIgualOuMaiorQueContratado_DeveRetornarVencidoPorGalonagem()
    {
        // Regra de negócio (contexto seção 9): vencido por galonagem = chip vermelho,
        // barra de galonagem VERDE (a apresentação nunca usa barra vermelha nesse caso —
        // isso é responsabilidade da camada de apresentação/query, aqui só validamos a situação).
        // ============ ARRANGE ============
        var contrato = CriarContratoValido(inicio: new DateOnly(2024, 1, 1), fim: new DateOnly(2026, 1, 1), volumeMensal: 100m);
        contrato.AtualizarGalonagemFaturada(2400m); // 100% do contratado (24 meses * 100)

        // ============ ACT ============
        var (situacao, _, saldo, percentual) = contrato.CalcularGalonagem(new DateOnly(2025, 1, 1));

        // ============ ASSERT ============
        Assert.AreEqual(SituacaoPcr.VencidoPorGalonagem, situacao);
        Assert.AreEqual(0m, saldo);
        Assert.AreEqual(1m, percentual);
    }

    [TestMethod]
    public void CalcularGalonagem_ComPrazoExpiradoESemCumprirGalonagem_DeveRetornarVencidoPorData()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido(inicio: new DateOnly(2020, 1, 1), fim: new DateOnly(2024, 1, 1), volumeMensal: 100m);
        contrato.AtualizarGalonagemFaturada(1000m); // bem abaixo do contratado

        // ============ ACT ============
        var (situacao, _, _, _) = contrato.CalcularGalonagem(new DateOnly(2026, 6, 15));

        // ============ ASSERT ============
        Assert.AreEqual(SituacaoPcr.VencidoPorData, situacao);
    }

    [TestMethod]
    public void RegistrarEvento_EmContratoEncerrado_DeveLancarDomainException()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido();
        contrato.RegistrarEvento(TipoEventoContrato.Encerramento, new DateOnly(2026, 1, 1), "Encerramento do contrato.");

        // ============ ACT / ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(() =>
            contrato.RegistrarEvento(TipoEventoContrato.Renovacao, new DateOnly(2026, 2, 1), "Tentativa após encerrado."));
        StringAssert.Contains(ex.Message, "contrato encerrado");
    }

    [TestMethod]
    public void RegistrarEvento_ComTipoEncerramento_DeveMarcarContratoComoEncerrado()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido();

        // ============ ACT ============
        contrato.RegistrarEvento(TipoEventoContrato.Encerramento, new DateOnly(2026, 1, 1), "Encerramento do contrato.");

        // ============ ASSERT ============
        Assert.IsTrue(contrato.Encerrado);
        Assert.AreEqual(2, contrato.Eventos.Count);
    }

    [TestMethod]
    public void RegistrarEvento_ComTipoDenuncia_DeveMarcarContratoComoEmDenuncia()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido();

        // ============ ACT ============
        contrato.RegistrarEvento(TipoEventoContrato.Denuncia, new DateOnly(2026, 1, 1), "Denúncia registrada.");

        // ============ ASSERT ============
        Assert.IsTrue(contrato.Denuncia);
        Assert.IsFalse(contrato.Encerrado);
    }

    [TestMethod]
    public void AtualizarGalonagemFaturada_ComValorNegativo_DeveLancarDomainException()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido();

        // ============ ACT / ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(() => contrato.AtualizarGalonagemFaturada(-1m));
        StringAssert.Contains(ex.Message, "não pode ser negativa");
    }
}
