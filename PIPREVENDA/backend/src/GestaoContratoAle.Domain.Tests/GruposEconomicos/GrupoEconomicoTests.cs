using PortalAle.Domain.Exceptions;
using PortalAle.Domain.GruposEconomicos;

namespace GestaoContratoAle.Domain.Tests.GruposEconomicos;

[TestClass]
public class GrupoEconomicoTests
{
    [TestMethod]
    public void Criar_ComCodigoDeTresDigitos_DeveCriarComSucesso()
    {
        // ============ ARRANGE / ACT ============
        var grupo = new GrupoEconomico("123", "Grupo Solaris");

        // ============ ASSERT ============
        Assert.AreEqual("123", grupo.Codigo);
        Assert.AreEqual("Grupo Solaris", grupo.Nome);
    }

    [TestMethod]
    [DataRow("12")]
    [DataRow("1234")]
    [DataRow("12a")]
    public void Criar_ComCodigoForaDoPadraoDeTresDigitos_DeveLancarDomainException(string codigoInvalido)
    {
        // ============ ARRANGE / ACT ============
        void Acao() => new GrupoEconomico(codigoInvalido, "Grupo Solaris");

        // ============ ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(Acao);
        StringAssert.Contains(ex.Message, "3 dígitos");
    }

    [TestMethod]
    public void Criar_ComNomeVazio_DeveLancarDomainException()
    {
        // ============ ARRANGE / ACT ============
        void Acao() => new GrupoEconomico("123", "");

        // ============ ASSERT ============
        var ex = Assert.ThrowsException<DomainException>(Acao);
        StringAssert.Contains(ex.Message, "Nome é obrigatório");
    }
}
