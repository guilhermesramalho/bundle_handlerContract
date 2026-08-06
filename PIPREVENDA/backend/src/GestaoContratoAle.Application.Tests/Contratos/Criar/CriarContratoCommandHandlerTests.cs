using Moq;
using PortalAle.Application.Contratos.Criar;
using PortalAle.Domain.Base;
using PortalAle.Domain.Clientes;
using PortalAle.Domain.Contratos;

namespace GestaoContratoAle.Application.Tests.Contratos.Criar;

[TestClass]
public class CriarContratoCommandHandlerTests
{
    private static CriarContratoRequest RequestValido() => new(
        ClienteId: 1,
        Pcr: "700.123",
        Segmento: Segmento.Rede,
        Tipo: TipoContrato.Pcvm,
        SituacaoMes: SituacaoMes.Ativo,
        Bandeira: "Bandeira ALE",
        RegistradoAle: true,
        InicioVigencia: new DateOnly(2024, 1, 1),
        FimVigencia: new DateOnly(2026, 1, 1),
        VolumeMensalContratado: 400m,
        MargemBase: 0.45m,
        Diretoria: "Sudeste",
        RegionalVendas: "GR São Paulo Interior",
        PontoVenda: "RN Campinas",
        Consultor: "Marcos Vidal");

    [TestMethod]
    public async Task ExecuteAsync_ComClienteInexistente_DeveRetornarFailure()
    {
        // ============ ARRANGE ============
        var contratoRepositoryMock = new Mock<IContratoRepository>();
        var clienteRepositoryMock = new Mock<IRepository<Cliente>>();
        clienteRepositoryMock
            .Setup(r => r.ExisteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CriarContratoCommandHandler(contratoRepositoryMock.Object, clienteRepositoryMock.Object);

        // ============ ACT ============
        var result = await handler.ExecuteAsync(RequestValido(), CancellationToken.None);

        // ============ ASSERT ============
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Errors[0], "não foi encontrado");
        contratoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task ExecuteAsync_ComPcrJaCadastrado_DeveRetornarFailure()
    {
        // ============ ARRANGE ============
        var contratoRepositoryMock = new Mock<IContratoRepository>();
        contratoRepositoryMock
            .Setup(r => r.ExistePorPcrAsync("700.123", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var clienteRepositoryMock = new Mock<IRepository<Cliente>>();
        clienteRepositoryMock
            .Setup(r => r.ExisteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CriarContratoCommandHandler(contratoRepositoryMock.Object, clienteRepositoryMock.Object);

        // ============ ACT ============
        var result = await handler.ExecuteAsync(RequestValido(), CancellationToken.None);

        // ============ ASSERT ============
        Assert.IsFalse(result.IsSuccess);
        StringAssert.Contains(result.Errors[0], "Já existe um contrato");
    }

    [TestMethod]
    public async Task ExecuteAsync_ComDadosValidos_DeveCriarContratoERetornarSuccess()
    {
        // ============ ARRANGE ============
        var contratoRepositoryMock = new Mock<IContratoRepository>();
        contratoRepositoryMock
            .Setup(r => r.ExistePorPcrAsync("700.123", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        contratoRepositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<Contrato>(), It.IsAny<CancellationToken>()))
            .Callback<Contrato, CancellationToken>((c, _) => typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(c, 42))
            .ReturnsAsync(42);

        var clienteRepositoryMock = new Mock<IRepository<Cliente>>();
        clienteRepositoryMock
            .Setup(r => r.ExisteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CriarContratoCommandHandler(contratoRepositoryMock.Object, clienteRepositoryMock.Object);

        // ============ ACT ============
        var result = await handler.ExecuteAsync(RequestValido(), CancellationToken.None);

        // ============ ASSERT ============
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value!.Id);
        Assert.AreEqual("700.123", result.Value.Pcr);
        contratoRepositoryMock.Verify(
            r => r.AdicionarAsync(It.Is<Contrato>(c => c.Pcr == "700.123" && c.ClienteId == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
