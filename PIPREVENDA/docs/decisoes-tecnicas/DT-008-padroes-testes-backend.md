# DT-008: Estratégia de Testes Unitários para Backend

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Definir estratégia de testes unitários como abordagem preferencial, ferramentas obrigatórias (MSTest, Moq, Faker) e cobertura por camada acima de (70%)
>
> **Quando usar:** Ao implementar testes em qualquer camada do backend, configurar novos projetos de teste ou revisar decisões sobre escopo e ferramental de testes
>
> **Palavras-chave:** `mstest` `testes-unitarios` `moq` `faker` `cobertura` `estrategia-testes` `testes` `testes-integração` `bogus`

---

## Contexto

O backend PortalAle é construído com Clean Architecture (Domain, Application, Data, Api), usando CQRS, Result Pattern e integrações com sistemas externos (ver skills `integracao-sap`/`integracao-elaw` para os contratos específicos do PIPREVENDA-1680). A equipe identificou os seguintes desafios relacionados à estratégia de testes:

1. **Complexidade de testes de integração**: Configuração de infraestrutura (banco de dados, mocks de APIs externas) aumenta significativamente o tempo de implementação e manutenção
2. **Dependências externas**: Testes que dependem de serviços externos (AIDA, APIM) são frágeis e lentos
3. **Baixa cobertura atual**: Ausência de testes em camadas críticas (Domínio, Aplicação, Infraestrutura)
4. **Custo vs. benefício**: Testes de integração complexos oferecem retorno marginal comparado à cobertura unitária

**Necessidade**: Simplificar a implementação de testes, reduzir dependências externas de infraestrutura e maximizar a cobertura de código com menor esforço, priorizando testes unitários com uso de mocks.

---

## Decisão

**Adotar testes unitários como estratégia preferencial para todas as camadas do backend**, com uso obrigatório de **MSTest** (framework), **Moq** (biblioteca de mocking) e **Bogus** (geração de dados falsos).

### Ferramentas Obrigatórias

| Ferramenta | Finalidade              | Uso Obrigatório                         |
| ---------- | ----------------------- | --------------------------------------- |
| **MSTest** | Framework de testes     | Todos os projetos de teste              |
| **Moq**    | Biblioteca de mocking   | Isolar dependências em testes unitários |
| **Bogus**  | Geração de dados falsos | Criar objetos de teste realistas        |

### Justificativa da Estratégia

- **Simplicidade**: Testes unitários não exigem configuração de infraestrutura (banco de dados, containers, APIs externas)
- **Velocidade**: Execução em milissegundos (vs. segundos/minutos em testes de integração)
- **Manutenibilidade**: Mocks permitem isolar componentes e testar cenários específicos sem dependências externas
- **Cobertura eficiente**: 70-75% de cobertura unitária oferece retorno superior ao investimento em testes de integração complexos

---

## Estratégia de Testes por Camada

### 1. **PortalAle.Domain**

**Escopo:**

- ✅ Todas as entidades de domínio
- ✅ Serviços de domínio
- ✅ Validações de negócio
- ✅ Classes utilitárias

**Tipo de Teste:** Unitário  
**Framework:** MSTest  
**Cobertura Mínima:** 70%

**Observações:**

- Focar em regras de negócio encapsuladas nas entidades
- Testar validações de domínio (regras invariantes)
- Validar comportamento de métodos que implementam lógica de negócio

---

### 2. **PortalAle.Application**

**Escopo:**

- ✅ **CommandHandlers** (operações de escrita/CQRS)
- ✅ **Classes utilitárias** (TextoUtils, IdUtils, etc.)
- ❌ **QueryHandlers** (NÃO serão testados - apenas leitura, lógica mínima)

**Tipo de Teste:** Unitário  
**Framework:** MSTest  
**Cobertura Mínima:** 70%

**Observações:**

- Focar em lógica de orquestração e transformação de dados
- **Mockar todas as dependências** (repositories, services externos)
- QueryHandlers são excluídos pois contêm apenas lógica de leitura/projeção sem regras de negócio complexas

---

### 3. **PortalAle.Data**

#### 3.1. **Serviços de Integração - AIDA**

**Escopo:**

- ✅ Classes de serviço que implementam integração com AIDA

**Tipo de Teste:** Unitário (com mocks)  
**Framework:** MSTest  
**Cobertura Mínima:** 70%

**Observações:**

- Focar em lógica de orquestração e transformação de dados
- **Isolar acoplamento com banco de dados**: Extrair lógica de conexão/consulta em métodos `protected virtual` para permitir override por mocks em testes
- Mockar dependências de repositórios e DbContext

---

#### 3.2. **Serviços de Integração - APIM**

**Escopo:**

- ✅ Serviços que consomem APIs externas via APIM
- ✅ Serviços de autenticação externa

**Tipo de Teste:** Unitário (com mocks de HttpClient)  
**Framework:** MSTest  
**Cobertura Mínima:** 70%

**Observações:**

- Focar em lógica de orquestração, transformação de dados e tratamento de erros
- **Isolar acoplamento HTTP**: Extrair chamadas HttpClient em métodos `protected virtual` para permitir override por mocks em testes
- Mockar respostas HTTP usando `HttpMessageHandler` fake ou extraindo interface `IHttpClientWrapper`

---

#### 3.3. **Repositórios (Implementações Concretas)**

**Escopo:**

- ❌ Todas as classes de repositório (herdam de `IRepository`)

**Tipo de Teste:** **NÃO TERÁ TESTES**

**Justificativa:**

- Repositórios dependem diretamente do EF Core e banco de dados
- Contêm apenas lógica de persistência (mapeamento ORM)
- Testar repositórios exigiria testes de integração complexos com banco de dados real ou in-memory
- Lógica de negócio deve estar na camada de Domínio/Aplicação, não em repositórios

---

#### 3.4. **Classes Utilitárias e Outros Serviços**

**Escopo:**

- ✅ Classes de mapeamento/conversão
- ✅ Helpers diversos
- ✅ Interceptors
- ✅ Serviços de cache
- ✅ Outros serviços de infraestrutura com lógica

**Tipo de Teste:** Unitário  
**Framework:** MSTest  
**Cobertura Mínima:** 70%

---

#### 3.5. **HealthChecks**

**Escopo:**

- ❌ `ControleAcessoHealthCheck`
- ❌ Outros health checks customizados

**Tipo de Teste:** **NÃO TERÁ TESTES**

**Justificativa:**

- Health checks são validados em execução real (monitoramento)
- Testar health checks requer infraestrutura real (banco, APIs externas)
- Retorno marginal comparado ao esforço de implementação

---

#### 3.6. **Scripts SQL e Migrations**

**Escopo:**

- ❌ Migrations do EF Core

**Tipo de Teste:** **NÃO TERÁ TESTES AUTOMATIZADOS**

**Justificativa:**

- Scripts SQL são validados manualmente durante desenvolvimento
- Migrations são testadas em ambientes de homologação antes de produção
- Complexidade de setup (banco de dados, dados seed) não justifica testes automatizados

---

### 4. **PortalAle.Api**

#### 4.1. **Segurança**

**Escopo:**

- ✅ `ClaimsProcessor`
- ✅ `PermissionUtils`
- ✅ `ProjectMemberRequirement`
- ✅ Handlers de autorização customizados

**Tipo de Teste:** Unitário  
**Framework:** MSTest  
**Cobertura Mínima:** 70%  
**Status:** ⚠️ **DESATIVADO INICIALMENTE**

**Justificativa para Desativação:**

- Aguardando refatorações do modelo de autenticação/autorização
- Testes serão implementados após estabilização das definições de segurança

---

#### 4.2. **Middleware e Extensions**

**Escopo:**

- ✅ Middleware customizados
- ✅ Extension methods customizados

**Tipo de Teste:** Unitário  
**Framework:** MSTest  
**Cobertura Mínima:** 70%

**Observações:**

- Mockar dependências do pipeline HTTP (`HttpContext`, `RequestDelegate`)
- Validar comportamento de transformação de requisições/respostas

---

#### 4.3. **Endpoints (Minimal APIs)**

**Escopo:**

- ✅ Todos os endpoints em `Endpoints/`

**Tipo de Teste:** **Testes de Arquitetura**  
**Framework:** MSTest + bibliotecas de testes arquiteturais
**Cobertura:** N/A (validação de conformidade arquitetural)

**Observações:**

- Testes serão implementados pelo **Arquiteto de Soluções**
- Validações obrigatórias:
  - Todos os endpoints possuem `.RequireAuthorization()`
  - Autenticação está configurada corretamente
  - Políticas de autorização estão aplicadas onde necessário
- **NÃO são testes unitários tradicionais** - focam em validar conformidade arquitetural

---

#### 4.4. **DTOs (Data Transfer Objects)**

**Escopo:**

- ❌ `ApiResponse`
- ❌ `CriarAcaoRequest`
- ❌ Todos os DTOs em `DTOs/`

**Tipo de Teste:** **NÃO TERÁ TESTES**

**Justificativa:**

- DTOs são POCOs (Plain Old CLR Objects) sem lógica de negócio
- Contêm apenas propriedades de transporte de dados
- Validações de DTOs são testadas indiretamente nos testes de endpoints/handlers

---

#### 4.5. **Program.cs e Configuração**

**Escopo:**

- ❌ `Program.cs`
- ❌ Classes de configuração de DI (Dependency Injection)

**Tipo de Teste:** **NÃO TERÁ TESTES**

**Justificativa:**

- Configuração de infraestrutura e pipeline de aplicação
- Validado em testes de integração (quando/se implementados)
- Complexidade de setup não justifica testes unitários

---

## Resumo de Cobertura por Projeto

| Projeto                    | Componentes Testados             | Tipo de Teste    | Framework           | Cobertura Mínima | Status                                 |
| -------------------------- | -------------------------------- | ---------------- | ------------------- | ---------------- | -------------------------------------- |
| **PortalAle.Domain**        | Entidades, serviços, validações  | Unitário         | MSTest, Moq, Faker  | 70%              | ✅ Implementar                         |
| **PortalAle.Application**      | CommandHandlers, utilitários     | Unitário         | MSTest, Moq, Faker  | 70%              | ✅ Implementar                         |
| **PortalAle.Data** | Serviços AIDA, APIM, utilitários | Unitário (mocks) | MSTest, Moq, Faker  | 70%              | ✅ Implementar                         |
| **PortalAle.Data** | Repositórios                     | -                | -                   | N/A              | ❌ Não testado                         |
| **PortalAle.Data** | HealthChecks, Scripts SQL        | -                | -                   | N/A              | ❌ Não testado                         |
| **PortalAle.Api**         | Middleware, Extensions           | Unitário         | MSTest, Moq         | 70%              | ✅ Implementar                         |
| **PortalAle.Api**         | Segurança                        | Unitário         | MSTest, Moq         | 70%              | ⚠️ Desativado (aguardando refatoração) |
| **PortalAle.Api**         | Endpoints                        | Arquitetura      | MSTest, NetArchTest | N/A              | 🔧 Arquiteto                           |
| **PortalAle.Api**         | DTOs, Program.cs                 | -                | -                   | N/A              | ❌ Não testado                         |

---

## Alternativas Consideradas

### 1. Testes de Integração como Estratégia Principal

**Descrição**: Priorizar testes de integração (com banco de dados real/in-memory) ao invés de testes unitários.

**Vantagens:**

- Testa fluxo completo end-to-end (endpoint → handler → repository → database)
- Valida integração entre camadas
- Detecta problemas de configuração e mapeamento ORM

**Desvantagens:**

- **Setup complexo**: Requer configuração de banco de dados (SQLite in-memory ou TestContainers/PostgreSQL)
- **Lentidão**: Testes de integração levam segundos vs milissegundos (unitários)
- **Fragilidade**: Dependem de infraestrutura externa (banco, APIs mock)
- **Manutenção custosa**: Mudanças em schema exigem atualização de dados seed
- **Baixa cobertura de cenários**: Difícil simular todos os edge cases (timeouts, falhas de rede, etc.)

**Por que foi rejeitada**: O custo de implementação e manutenção de testes de integração não justifica o retorno comparado à cobertura unitária. Testes unitários com mocks oferecem melhor relação custo/benefício.

---

### 2. XUnit ao invés de MSTest

**Descrição**: Usar XUnit como framework de testes.

**Vantagens:**

- Mais moderno e idiomático
- Melhor suporte para testes paralelos
- Comunidade maior no .NET

**Desvantagens:**

- Equipe já familiarizada com MSTest
- Necessidade de migração de testes existentes (PortalAle.Api.Tests usa MSTest)
- Integração com Visual Studio menos nativa que MSTest
- MSTest é framework oficial da Microsoft

**Por que foi rejeitada**: MSTest é framework padrão da Microsoft, bem integrado com Visual Studio e VS Code. Mudança não justifica o esforço de migração e aprendizado.

---

## Consequências

### Positivas

- ✅ **Simplicidade de implementação**: Testes unitários não exigem setup de infraestrutura (banco de dados, containers, APIs mock), acelerando desenvolvimento
- ✅ **Velocidade de execução**: Testes rodam em milissegundos, fornecendo feedback instantâneo durante desenvolvimento
- ✅ **Cobertura eficiente**: 70% de cobertura unitária oferece excelente retorno sobre investimento sem overhead de testes de integração
- ✅ **Isolamento total**: Mocks (Moq) permitem testar componentes isoladamente, facilitando identificação de bugs
- ✅ **Facilita refatoração**: Testes unitários protegem contra regressões durante mudanças de código
- ✅ **CI/CD ágil**: Pipelines executam testes em segundos (vs minutos em testes de integração)

### Negativas

- ⚠️ **Risco em repositórios**: Repositórios não serão testados, podendo conter bugs em queries complexas
- ⚠️ **Necessidade de disciplina**: Desenvolvedores precisam mockar corretamente dependências (risco de mocks que não refletem comportamento real)
- ⚠️ **Curva de aprendizado**: Equipe precisa dominar Moq (setup, verify, callbacks) e Faker
- ⚠️ **Manutenção de mocks**: Mudanças em interfaces exigem atualização de mocks em múltiplos testes

---

## Implementação

### 1. Estrutura de Projetos de Teste

Os testes devem ser estruturados de forma a espelhar a estrutura de pastas do projeto principal que está sendo testado.

Os projetos de teste já existem na solução. Estrutura atual:

```
src/
├── PortalAle.Domain.Tests/           # Testes unitários de entidades/domínio
│   ├── Enums/
│   │   └── TipoInformacaoCodigoTests.cs
│   └── PortalAle.Domain.Tests.csproj
│   ├── ProjetoTests.cs              # Testa Projeto.cs
│   ├── StatusTests.cs               # Testa Status.cs
│   ├── FaseTests.cs                 # Testa Fase.cs
│   ├── GerenciaxareaTests.cs        # Testa Gerenciaxarea.cs
│   ├── InformacaoCATests.cs         # Testa InformacaoCA.cs
│   ├── Base/
│       └── EntityTests.cs           # Testa Entity.cs base
│
├── PortalAle.Application.Tests/         # Testes unitários de handlers/aplicação
│   └── PortalAle.Application.Tests.csproj
│   ├── Projetos/
│   │   ├── Criar/
│   │   │   └── CriarProjetoCommandHandlerTests.cs
│   │   ├── Atualizar/
│   │   │   └── AtualizarProjetoCommandHandlerTests.cs
│   │   ├── Excluir/
│   │   │   └── ExcluirProjetoCommandHandlerTests.cs
│   │   └── Importar/
│   │       └── ImportarProjetoCommandHandlerTests.cs
│   ├── Status/
│   │   └── (sem Commands - apenas Queries, não testados)
│   ├── Fases/
│   ├── Contextos/
│   └── GerenciaXArea/
│
├── PortalAle.Data.Tests/    # Testes unitários de serviços/infraestrutura
│   ├── Repositorios/
│   │   └── ProjetoRepositoryTests.cs  # (repositórios NÃO testados por decisão)
│   ├── Services/
│   │   └── ContextValidationServiceTests.cs
│   └── PortalAle.Data.Tests.csproj
│   ├── Services/
│   │   ├── AidaConnectionServiceTests.cs      # Mock de conexão AIDA
│   │   ├── AidaProjetoServiceTests.cs         # Mock de integração AIDA
│   │   ├── AidaPocoServiceTests.cs            # Mock de integração AIDA
│   │   ├── SincronizacaoAidaProjetosServiceTests.cs
│   │   └── AwsSecretsServiceTests.cs          # Mock de AWS Secrets
│
└── PortalAle.Api.Tests/            # Testes de endpoints/WebAPI/integração
    ├── Endpoints/
    │   ├── AuthEndpointsTests.cs
    │   └── ProjetosEndpointsTests.cs
    ├── Seguranca/
    │   └── PermissionUtilsTests.cs
    └── PortalAle.Api.Tests.csproj
    ├── Endpoints/           # Testes ARQUITETURAIS
    │   ├── StatusEndpointsTests.cs
    │   ├── FasesEndpointsTests.cs
    │   ├── ContextoEndpointsTests.cs
    │   ├── GerenciaXAreaEndpointsTests.cs
    │   └── (outros endpoints)
    ├── Seguranca/           # Testes UNITÁRIOS
    │   ├── ClaimsUtilsTests.cs
    │   ├── ProjectMemberRequirementTests.cs
    │   └── (após refatoração de segurança)
    └── Extensions/          # Testes UNITÁRIOS
        └── DatabaseExtensionsTests.cs
```

**Observações:**

- **IMPORTANTE**: A estrutura de testes deve espelhar exatamente a organização do código de produção:
  - `PortalAle.Domain.Tests/` → espelha `PortalAle.Domain/` (entidades na raiz, pastas Base/, Enums/)
  - `PortalAle.Application.Tests/` → espelha `PortalAle.Application/` (por domínio/operação: Projetos/Criar/, etc.)
  - `PortalAle.Data.Tests/` → espelha `PortalAle.Data/` (Services/, Repositorios/)
  - `PortalAle.Api.Tests/` → espelha `PortalAle.Api/` (Endpoints/, Seguranca/, Extensions/)
- Testes de repositórios existem mas NÃO devem ser expandidos (conforme decisão na seção 3.3)
- QueryHandlers NÃO devem ser testados (conforme decisão na seção 2)

---

### 2. Padrões de Implementação

#### 3.1. Estrutura AAA (Arrange-Act-Assert)

Todos os testes DEVEM seguir o padrão AAA com comentários explicativos:

```csharp
[TestMethod]
public async Task CriarProjetoAsync_ComDadosValidos_DeveCriarProjeto()
{
    // ============ ARRANGE ============
    // Preparar dados de entrada usando Faker
    var faker = new Faker("pt_BR");
    var command = new CriarProjetoCommand
    {
        Nome = faker.Company.CompanyName(),
        StatusId = 1,
        FaseId = 1
    };

    // Configurar mocks de dependências
    var repositoryMock = new Mock<IProjetoRepository>();
    repositoryMock
        .Setup(r => r.AdicionarAsync(It.IsAny<Projeto>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Projeto p, CancellationToken ct) => p);

    var handler = new CriarProjetoCommandHandler(repositoryMock.Object);

    // ============ ACT ============
    // Executar o método sob teste
    var result = await handler.ExecuteAsync(command, CancellationToken.None);

    // ============ ASSERT ============
    // Verificar resultado
    Assert.IsTrue(result.IsSuccess);
    Assert.IsNotNull(result.Value);
    Assert.AreEqual(command.Nome, result.Value.Nome);

    // Verificar interações com mocks
    repositoryMock.Verify(
        r => r.AdicionarAsync(It.Is<Projeto>(p => p.Nome == command.Nome), It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

---

#### 3.2. Nomenclatura de Testes

**Formato obrigatório**: `[Método]_[Cenário]_[ResultadoEsperado]`

**Exemplos:**

```csharp
// ✅ CORRETO
ObterPorIdAsync_ComIdValido_DeveRetornarProjeto()
ObterPorIdAsync_ComIdInexistente_DeveRetornarNull()
CriarProjetoAsync_ComNomeVazio_DeveRetornarErro()
AtualizarStatusAsync_ComStatusInvalido_DeveRetornarFailure()

// ❌ INCORRETO
Test1()                    // Não descritivo
TestCriarProjeto()         // Não indica cenário
ProjetoFunciona()          // Vago
Teste_Criar_Projeto()      // Não indica resultado esperado
```

---

#### 3.3. Uso de Bogus

**Gerar dados realistas para testes:**

```csharp
// Importar namespace
using Bogus;

// Criar instância Faker
var faker = new Faker("pt_BR");  // Dados em português brasileiro

// Gerar dados
var nome = faker.Person.FullName;           // "João Silva"
var email = faker.Internet.Email();         // "[email protected]"
var dataInicio = faker.Date.Recent(30);     // Data dos últimos 30 dias
var descricao = faker.Lorem.Sentence();     // Frase aleatória

// Gerar objeto complexo
var projeto = new Projeto
{
    Nome = faker.Company.CompanyName(),
    Descricao = faker.Lorem.Paragraph(),
    DataInicio = faker.Date.Recent(10),
    StatusId = faker.Random.Int(1, 5)
};

// Gerar lista de objetos
var projetos = new Faker<Projeto>("pt_BR")
    .RuleFor(p => p.Nome, f => f.Company.CompanyName())
    .RuleFor(p => p.Descricao, f => f.Lorem.Paragraph())
    .RuleFor(p => p.DataInicio, f => f.Date.Recent(30))
    .Generate(10);  // Gera 10 projetos
```

---

#### 3.4. Uso de Moq

**Mockar dependências em testes unitários:**

```csharp
// 1. Criar mock de interface
var repositoryMock = new Mock<IProjetoRepository>();

// 2. Configurar comportamento (método retorna valor)
repositoryMock
    .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Projeto { Id = 1, Nome = "Teste" });

// 3. Configurar comportamento (método executa callback)
repositoryMock
    .Setup(r => r.AdicionarAsync(It.IsAny<Projeto>(), It.IsAny<CancellationToken>()))
    .Callback<Projeto, CancellationToken>((p, ct) => p.Id = 123)
    .ReturnsAsync((Projeto p, CancellationToken ct) => p);

// 4. Configurar comportamento (método lança exceção)
repositoryMock
    .Setup(r => r.RemoverAsync(999, It.IsAny<CancellationToken>()))
    .ThrowsAsync(new KeyNotFoundException("Projeto não encontrado"));

// 5. Injetar mock no construtor
var handler = new CriarProjetoCommandHandler(repositoryMock.Object);

// 6. Verificar chamadas ao mock
repositoryMock.Verify(
    r => r.AdicionarAsync(It.IsAny<Projeto>(), It.IsAny<CancellationToken>()),
    Times.Once  // Chamado exatamente uma vez
);

repositoryMock.Verify(
    r => r.ObterPorIdAsync(It.Is<int>(id => id > 0), It.IsAny<CancellationToken>()),
    Times.AtLeastOnce  // Chamado pelo menos uma vez
);

repositoryMock.VerifyNoOtherCalls();  // Nenhuma outra chamada foi feita
```

---

#### 3.5. Mockar Serviços com HttpClient (APIM)

**Isolar chamadas HTTP em serviços de integração:**

**Opção 1: Extrair método protegido virtual**

```csharp
// Serviço de produção
public class ApimService
{
    private readonly HttpClient _httpClient;

    public ApimService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DadosExternos> ObterDadosAsync(int id)
    {
        // Lógica de transformação/validação
        var url = $"/api/external/{id}";

        // Chamada HTTP isolada em método virtual
        var response = await ExecutarRequisicaoHttpAsync(url);

        // Lógica de transformação de resposta
        var dados = JsonSerializer.Deserialize<DadosExternos>(response);
        return dados;
    }

    // Método virtual permite override em testes
    protected virtual async Task<string> ExecutarRequisicaoHttpAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

// Classe de teste
public class ApimServiceTests
{
    [TestMethod]
    public async Task ObterDadosAsync_ComIdValido_DeveRetornarDados()
    {
        // Arrange
        var mockService = new Mock<ApimService>(null) { CallBase = true };
        mockService
            .Setup(s => s.ExecutarRequisicaoHttpAsync(It.IsAny<string>()))
            .ReturnsAsync("{\"id\":1,\"nome\":\"Teste\"}");

        // Act
        var resultado = await mockService.Object.ObterDadosAsync(1);

        // Assert
        Assert.AreEqual(1, resultado.Id);
        Assert.AreEqual("Teste", resultado.Nome);
    }
}
```

**Opção 2: Mockar HttpMessageHandler**

```csharp
// Classe helper para criar HttpClient fake
public static class HttpClientFactory
{
    public static HttpClient CreateFakeClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent)
            });

        return new HttpClient(handlerMock.Object);
    }
}

// Uso em teste
[TestMethod]
public async Task ObterDadosAsync_ComIdValido_DeveRetornarDados()
{
    // Arrange
    var httpClient = HttpClientFactory.CreateFakeClient("{\"id\":1,\"nome\":\"Teste\"}");
    var service = new ApimService(httpClient);

    // Act
    var resultado = await service.ObterDadosAsync(1);

    // Assert
    Assert.AreEqual(1, resultado.Id);
    Assert.AreEqual("Teste", resultado.Nome);
}
```

---

#### 3.6. Mockar Serviços com Acesso a Banco (AIDA)

**Isolar consultas ao banco em métodos virtuais:**

```csharp
// Serviço de produção
public class AidaService
{
    private readonly ApplicationDbContext _context;

    public AidaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UsuarioAida>> ObterUsuariosAtivosAsync()
    {
        // Lógica de validação/preparação
        var dataLimite = DateTime.Now.AddMonths(-6);

        // Consulta isolada em método virtual
        var usuarios = await ObterUsuariosDoBancoAsync(dataLimite);

        // Lógica de transformação
        return usuarios.Where(u => u.Ativo).ToList();
    }

    // Método virtual permite override em testes
    protected virtual async Task<List<UsuarioAida>> ObterUsuariosDoBancoAsync(DateTime dataLimite)
    {
        return await _context.UsuariosAida
            .Where(u => u.UltimoAcesso >= dataLimite)
            .ToListAsync();
    }
}

// Teste
[TestMethod]
public async Task ObterUsuariosAtivosAsync_ComUsuariosAtivos_DeveRetornarLista()
{
    // Arrange
    var faker = new Faker<UsuarioAida>("pt_BR")
        .RuleFor(u => u.Nome, f => f.Person.FullName)
        .RuleFor(u => u.Ativo, f => true)
        .RuleFor(u => u.UltimoAcesso, f => f.Date.Recent(30));

    var usuariosMock = faker.Generate(5);

    var mockService = new Mock<AidaService>(null) { CallBase = true };
    mockService
        .Setup(s => s.ObterUsuariosDoBancoAsync(It.IsAny<DateTime>()))
        .ReturnsAsync(usuariosMock);

    // Act
    var resultado = await mockService.Object.ObterUsuariosAtivosAsync();

    // Assert
    Assert.AreEqual(5, resultado.Count);
    Assert.IsTrue(resultado.All(u => u.Ativo));
}
```

---

### 4. Execução de Testes

**Executar todos os testes:**

```powershell
# Executar todos os testes da solução
dotnet test

# Executar testes de um projeto específico
dotnet test PortalAle.Domain.Tests

# Executar com verbosidade detalhada
dotnet test --verbosity detailed

# Executar testes com coleta de cobertura
dotnet test --collect:"XPlat Code Coverage"

# Gerar relatório HTML de cobertura
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

**Executar testes específicos:**

```powershell
# Filtrar por nome do teste
dotnet test --filter "FullyQualifiedName~CriarProjetoCommandHandlerTests"

# Filtrar por categoria (se usar [TestCategory])
dotnet test --filter "TestCategory=Integration"
```

---

## Verificação de Conformidade

Use este checklist para auditar se a estratégia de testes está sendo seguida corretamente:

### Padrões de Código

- [ ] Estrutura de pastas espelha organização do projeto de produção (ex: `Projetos/`, `Contextos/`)
- [ ] Todos os testes seguem nomenclatura `[Método]_[Cenário]_[ResultadoEsperado]`
- [ ] Todos os testes seguem padrão AAA (Arrange-Act-Assert) com comentários
- [ ] Dependências são mockadas usando **Moq** (não instâncias reais)
- [ ] Mocks são verificados com `.Verify()` para garantir interações corretas

### Cobertura

- [ ] **PortalAle.Domain**: Cobertura ≥ 70%
- [ ] **PortalAle.Application**: Cobertura ≥ 70% (CommandHandlers testados, QueryHandlers NÃO testados)
- [ ] **PortalAle.Data**: Cobertura ≥ 70% (excluindo repositórios, health checks, migrations)
- [ ] **PortalAle.Api**: Cobertura ≥ 70% (excluindo DTOs, Program.cs, segurança temporariamente)

### Qualidade

- [ ] Todos os testes passam localmente antes de commit
- [ ] Não existem testes ignorados (`[Ignore]`) sem justificativa documentada
- [ ] Testes são independentes (não dependem de ordem de execução)
- [ ] Dados de teste são mínimos e focados no cenário testado
- [ ] Testes executam em < 5 minutos (localmente)

### Implementação por Camada

- [ ] **Domínio**: Entidades possuem testes de validações e regras de negócio
- [ ] **Aplicação**: CommandHandlers possuem testes unitários com mocks de repositórios
- [ ] **Aplicação**: QueryHandlers NÃO possuem testes (conforme decisão)
- [ ] **Infraestrutura**: Repositórios NÃO possuem testes (conforme decisão)
- [ ] **WebAPI**: Middleware e Extensions possuem testes unitários
- [ ] **WebAPI**: Endpoints possuem testes arquiteturais
- [ ] **WebAPI**: DTOs e Program.cs NÃO possuem testes (conforme decisão)
- [ ] **WebAPI**: Testes de segurança estão desativados (aguardando refatoração)

---

## Referências

- **DT-005**: Padrões de Estilo de Código Backend
- **DT-016**: Padrões Command/Query (CQRS) na Camada de Aplicação
- **Moq Repository**: https://github.com/moq/moq4
- **Bogus Repository**: https://github.com/bchavez/Bogus
- **MSTest Documentation**: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest
- **Code Coverage Tools**: https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage
