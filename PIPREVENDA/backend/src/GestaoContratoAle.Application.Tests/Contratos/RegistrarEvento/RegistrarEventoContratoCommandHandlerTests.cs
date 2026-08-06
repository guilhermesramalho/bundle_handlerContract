using Moq;
using PortalAle.Application.Contratos.RegistrarEvento;
using PortalAle.Domain.Contratos;

namespace GestaoContratoAle.Application.Tests.Contratos.RegistrarEvento;

[TestClass]
public class RegistrarEventoContratoCommandHandlerTests
{
    private static Contrato CriarContratoValido() => new(
        clienteId: 1,
        pcr: "700.123",
        segmento: Segmento.Rede,
        tipo: TipoContrato.Pcvm,
        situacaoMes: SituacaoMes.Ativo,
        bandeira: "Bandeira ALE",
        registradoAle: true,
        inicioVigencia: new DateOnly(2024, 1, 1),
        fimVigencia: new DateOnly(2026, 1, 1),
        volumeMensalContratado: 400m,
        margemBase: 0.45m,
        diretoria: "Sudeste",
        regionalVendas: "GR São Paulo Interior",
        pontoVenda: "RN Campinas",
        consultor: "Marcos Vidal");

    [TestMethod]
    public async Task ExecuteAsync_ComContratoInexistente_DeveRetornarFailure()
    {
        // ============ ARRANGE ============
        var repositoryMock = new Mock<IContratoRepository>();
        repositoryMock
            .Setup(r => r.ObterComEventosAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contrato?)null);

        var handler = new RegistrarEventoContratoCommandHandler(repositoryMock.Object);
        var request = new RegistrarEventoContratoRequest(999, TipoEventoContrato.Renovacao, new DateOnly(2026, 1, 1), "Renovação.");

        // ============ ACT ============
        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        // ============ ASSERT ============
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Errors[0], "não foi encontrado");
    }

    [TestMethod]
    public async Task ExecuteAsync_ComTipoEncerramento_DeveRegistrarEventoEAtualizarContratoEncerrado()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido();
        var repositoryMock = new Mock<IContratoRepository>();
        repositoryMock
            .Setup(r => r.ObterComEventosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        var handler = new RegistrarEventoContratoCommandHandler(repositoryMock.Object);
        var request = new RegistrarEventoContratoRequest(1, TipoEventoContrato.Encerramento, new DateOnly(2026, 1, 1), "Encerramento do contrato.");

        // ============ ACT ============
        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        // ============ ASSERT ============
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value!.ContratoEncerrado);
        repositoryMock.Verify(r => r.AtualizarAsync(contrato, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_EmContratoJaEncerrado_DeveRetornarFailure()
    {
        // ============ ARRANGE ============
        var contrato = CriarContratoValido();
        contrato.RegistrarEvento(TipoEventoContrato.Encerramento, new DateOnly(2025, 1, 1), "Encerramento anterior.");

        var repositoryMock = new Mock<IContratoRepository>();
        repositoryMock
            .Setup(r => r.ObterComEventosAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        var handler = new RegistrarEventoContratoCommandHandler(repositoryMock.Object);
        var request = new RegistrarEventoContratoRequest(1, TipoEventoContrato.Renovacao, new DateOnly(2026, 1, 1), "Tentativa inválida.");

        // ============ ACT ============
        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        // ============ ASSERT ============
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Errors[0], "contrato encerrado");
        repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Contrato>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
