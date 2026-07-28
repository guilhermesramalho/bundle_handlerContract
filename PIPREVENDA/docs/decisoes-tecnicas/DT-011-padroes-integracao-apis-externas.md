# DT-011: Padrões de Integração com APIs Externas

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Adotar Anti-Corruption Layer para integrações externas, isolando domínio de APIs voláteis (CAv4, AIDA, Força Trabalho)
>
> **Quando usar:** Ao integrar com APIs externas, criar adaptadores HTTP, implementar camada de proteção ou definir modelos de integração
>
> **Palavras-chave:** `apim` `aida` `anti-corruption-layer` `api-integration` `httpclient` `resiliencia`

## Contexto

O sistema Projeto Digital integra-se com múltiplas APIs externas (CAv4, AIDA, Força de Trabalho) para obter dados de outros sistemas corporativos.

**Desafios de integração com sistemas externos:**

1. **Acoplamento**: Modelos de dados externos não devem contaminar o domínio
2. **Volatilidade**: APIs externas mudam independentemente do nosso sistema
3. **Confiabilidade**: Sistemas externos podem estar indisponíveis ou lentos
4. **Testabilidade**: Dependências externas dificultam testes unitários
5. **Manutenibilidade**: Mudanças em APIs externas não devem impactar o domínio

**Princípios arquiteturais aplicados:**

- **Anti-Corruption Layer (DDD)**: Camada de proteção que traduz modelos externos para o domínio
- **Dependency Inversion (SOLID)**: Domínio depende de abstrações, não de implementações
- **Ports and Adapters**: Interfaces (portas) na aplicação, implementações (adaptadores) na infraestrutura

---

## Decisão

**Adotar Anti-Corruption Layer para todas as integrações externas**, implementado através de:

### 1. Separação em Camadas (Clean Architecture)

```
Domínio (Entities, Value Objects)
    ↑
    │ (nenhuma dependência)
    │
Aplicação (Services, Use Cases)
    ↑
    │ (depende de interfaces)
    │
Infraestrutura (Adaptadores HTTP)
    ↓
    API Externa
```

### 2. Componentes Obrigatórios

- **Interface de Serviço** (`Aplicacao/Services/I*Service.cs`)
  - Define contrato em termos do domínio
  - Retorna tipos do domínio (Entities/Value Objects)
  - Não expõe detalhes de infraestrutura (HTTP, JSON, etc.)

- **Implementação na Infraestrutura** (`Infraestrutura/<ServicoExterno>/*Service.cs`)
  - Usa `HttpClient` via `HttpClientFactory`
  - Traduz modelos externos → modelos do domínio (Anti-Corruption)
  - Trata erros de infraestrutura (timeout, 404, 500, etc.)

- **Configuração Tipada** (`Infraestrutura/<ServicoExterno>/*Config.cs`)
  - IOptions Pattern para configurações
  - Validação de configurações obrigatórias

### 3. Padrão de Nomenclatura

- Interface: `I{Sistema}Service` (ex: `IAidaProjetoService`)
- Implementação: `{Sistema}Service` (ex: `AidaProjetoService`)
- Configuração: `{Sistema}Config` (ex: `AidaConfig`)
- DTOs externos: `{Sistema}Dto` (privados na implementação)

---

## Alternativas Consideradas

### 1. Usar DTOs externos diretamente no domínio

**Rejeitado**: Viola DDD. O domínio ficaria acoplado aos modelos externos, dificultando evolução independente e testes.

### 2. Repository Pattern para APIs externas

**Rejeitado**: Repository é para agregados do domínio, não para serviços externos. APIs externas não são repositórios de dados.

### 3. Acesso direto via HttpClient no Application Layer

**Rejeitado**: Viola Dependency Inversion. Camada de aplicação não deve conhecer detalhes de infraestrutura HTTP.

### 4. Bibliotecas de terceiros (Refit, RestSharp)

**Considerado mas não adotado**: Adiciona abstrações desnecessárias. `HttpClientFactory` é suficiente para maioria dos casos. Avaliar apenas se houver complexidade significativa (webhooks, streaming, etc.).

### 5. Cliente genérico para todas as APIs

**Rejeitado**: Dificulta tipagem, tratamento de erros específico e evolução independente de cada integração.

---

## Consequências

### Positivos

✅ **Isolamento do Domínio**: Lógica de negócio protegida de mudanças em APIs externas  
✅ **Testabilidade**: Interfaces facilmente mockáveis em testes unitários  
✅ **Evolução Independente**: Mudanças em APIs externas não impactam domínio  
✅ **Clareza de Responsabilidades**: Separação clara entre domínio e infraestrutura  
✅ **Resiliência**: Tratamento de erros centralizado por integração

### Negativos

❌ **Boilerplate Inicial**: Necessário criar interface + implementação + config para cada API  
❌ **Mapeamento Manual**: Traduzir DTOs externos para entidades do domínio (não usar AutoMapper)  
❌ **Curva de Aprendizado**: Equipe precisa entender princípios de Clean Architecture e DDD

### Quando NÃO Usar Este Padrão

- APIs internas da própria organização com contratos estáveis
- Integrações temporárias ou provas de conceito
- Sistemas legados que precisam ser expostos como API (usar BFF/Gateway ao invés)

---

## Implementação

> **Referência**: Exemplos baseados em integração AIDA (sistema de projetos Petrobras)

### 1. Estrutura de Pastas

```
ProjSub.Aplicacao/
└── Services/
    └── IAidaProjetoService.cs        # Interface (Porta)

ProjSub.Infraestrutura/
└── Aida/
    ├── AidaProjetoService.cs         # Adaptador HTTP
    └── AidaConfig.cs                 # Configuração
```

### 2. Interface na Camada de Aplicação (Porta)

**Localização**: `ProjSub.Aplicacao/Services/IAidaProjetoService.cs`

```csharp
using ProjSub.Dominio.Projetos;

namespace ProjSub.Aplicacao.Services;

/// <summary>
/// Serviço de consulta ao AIDA (sistema de projetos Petrobras).
/// IMPORTANTE: Retorna tipos do domínio, NÃO DTOs externos.
/// </summary>
public interface IAidaProjetoService
{
    /// <summary>
    /// Busca projeto por código.
    /// </summary>
    /// <returns>Entidade Projeto do domínio ou null se não encontrado</returns>
    Task<Projeto?> BuscarProjetoPorCodigoAsync(string codigo, CancellationToken ct = default);
}
```

**Princípios aplicados:**

✅ Interface definida em termos do **domínio** (retorna `Projeto`, não `AidaProjetoResponse`)  
✅ Sem detalhes de infraestrutura (HTTP, endpoints, autenticação)  
✅ Localizada na camada de Aplicação (não no Domínio)

### 3. Implementação na Infraestrutura (Adaptador)

**Localização**: `ProjSub.Infraestrutura/Aida/AidaProjetoService.cs`

```csharp
using ProjSub.Aplicacao.Services;
using ProjSub.Dominio.Projetos;

namespace ProjSub.Infraestrutura.Aida;

public class AidaProjetoService : IAidaProjetoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AidaProjetoService> _logger;

    public AidaProjetoService(HttpClient httpClient, ILogger<AidaProjetoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Projeto?> BuscarProjetoPorCodigoAsync(string codigo, CancellationToken ct)
    {
        // 1. Chamar API externa
        var response = await _httpClient.GetAsync($"projetos/{codigo}", ct);

        // 2. Tratar erros (404 = não encontrado)
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        // 3. Deserializar
        var dto = await response.Content.ReadFromJsonAsync<AidaProjetoDto>(ct);

        // 4. ANTI-CORRUPTION: Traduzir DTO → Entidade do domínio
        return MapearParaDominio(dto);
    }

    // Anti-Corruption Layer
    private Projeto? MapearParaDominio(AidaProjetoDto? dto)
    {
        if (dto == null) return null;

        return new Projeto
        {
            Codigo = dto.CodigoProjeto,          // Traduzir nomenclatura AIDA → Domínio
            Nome = dto.NomeProjeto,
            Status = dto.StatusProjeto,
            DataInicio = dto.DtInicio
        };
    }

    // DTO privado - NÃO expor fora desta classe
    private record AidaProjetoDto(string CodigoProjeto, string NomeProjeto,
                                   string StatusProjeto, DateTime DtInicio);
}
```

**Princípios aplicados:**

✅ **Anti-Corruption Layer**: Método `MapearParaDominio` traduz nomenclatura externa  
✅ **Encapsulamento**: DTO privado (não vaza para fora)  
✅ **Separação**: Lógica HTTP isolada do domínio  
✅ **Tratamento de Erros**: HTTP 404 → `null`

### 4. Configuração (Options Pattern)

**Localização**: `ProjSub.Infraestrutura/Aida/AidaConfig.cs`

```csharp
namespace ProjSub.Infraestrutura.Aida;

public class AidaConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 10000;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new InvalidOperationException("Aida:BaseUrl obrigatório");
    }
}
```

### 5. Registro de Dependências (DI)

**Localização**: `ProjSub.Infraestrutura/DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfraestrutura(
    this IServiceCollection services, IConfiguration config)
{
    // Configurar options e validar
    services.Configure<AidaConfig>(config.GetSection("Aida"));
    services.AddOptions<AidaConfig>().Validate(c => c.Validate());

    // Registrar HttpClient com factory
    services.AddHttpClient<IAidaProjetoService, AidaProjetoService>((sp, client) =>
    {
        var cfg = sp.GetRequiredService<IOptions<AidaConfig>>().Value;
        client.BaseAddress = new Uri(cfg.BaseUrl);
        client.Timeout = TimeSpan.FromMilliseconds(cfg.TimeoutMs);
        client.DefaultRequestHeaders.Add("apikey", cfg.ApiKey);
    });

    return services;
}
```

### 6. Configuração (appsettings.json)

```json
{
  "Aida": {
    "BaseUrl": "https://api.aida.petrobras.com.br/v1",
    "ApiKey": "{{secrets-manager}}",
    "TimeoutMs": 10000
  }
}
```

### 7. Uso na Camada de Aplicação

```csharp
public class CriarSolicitacaoCommandHandler
{
    private readonly IAidaProjetoService _aidaService;

    public async Task<Result> ExecuteAsync(CriarSolicitacaoCommand cmd, CancellationToken ct)
    {
        // Buscar projeto via AIDA
        var projeto = await _aidaService.BuscarProjetoPorCodigoAsync(cmd.CodigoProjeto, ct);

        if (projeto == null)
            return Result.Failure("Projeto não encontrado no AIDA");

        // Lógica de negócio usando entidade do domínio
        var solicitacao = new Solicitacao
        {
            Projeto = projeto,  // Entidade do domínio, não DTO externo
            Status = StatusSolicitacao.Pendente
        };

        return Result.Success();
    }
}
```

**Princípios aplicados:**

✅ Depende de **interface**, não de implementação  
✅ Trabalha com **entidades do domínio** (`Projeto`), não DTOs  
✅ Não conhece detalhes de infraestrutura (HTTP, endpoints)

---

## Boas Práticas

### 1. Anti-Corruption Layer (Tradução de Modelos)

**✅ FAZER:**

- Criar métodos privados de mapeamento (ex: `MapearParaDominio`)
- DTOs externos devem ser `private` ou `internal` (nunca expostos)
- Retornar sempre tipos do domínio nas interfaces públicas

**❌ NÃO FAZER:**

- Usar AutoMapper (biblioteca com mudança de licenciamento)
- Expor DTOs externos via API ou interfaces públicas
- Retornar `HttpResponseMessage` ou tipos de infraestrutura

**Exemplo:**

```csharp
// ✅ CORRETO
public interface IAidaProjetoService
{
    Task<Projeto?> BuscarProjetoPorCodigoAsync(string codigo); // Retorna tipo do domínio
}

// ❌ ERRADO
public interface IAidaProjetoService
{
    Task<AidaProjetoDto?> BuscarProjetoPorCodigoAsync(string codigo); // Expõe DTO externo
}
```

### 2. Tratamento de Erros

**Regras:**

- **404 (Not Found)** → Retornar `null` (semântica: recurso não existe)
- **401/403 (Auth)** → Lançar `UnauthorizedAccessException`
- **429 (Rate Limit)** → Lançar `InvalidOperationException` com mensagem clara
- **500/503 (Server Error)** → Lançar `HttpRequestException` (será retentado se houver retry)
- **Timeout** → Lançar `TimeoutException`

**❌ NÃO FAZER:**

- Logar erros de validação ou regras de negócio (ver DT-007)
- Retornar exceções como valores de retorno
- Capturar todas as exceções e retornar `null` (oculta problemas)

### 3. Resiliência (Retry, Timeout, Circuit Breaker)

**Recomendações:**

| Padrão                | Quando Usar                                | Implementação        |
| --------------------- | ------------------------------------------ | -------------------- |
| **Retry com Backoff** | Erros transitórios (500, 503, timeout)     | Manual ou Polly      |
| **Circuit Breaker**   | APIs instáveis com falhas frequentes       | Manual ou Polly      |
| **Timeout**           | Todas as APIs (evitar bloqueios infinitos) | `HttpClient.Timeout` |
| **Cache**             | Dados relativamente estáticos              | `IMemoryCache` + TTL |

**Timeout adequado por tipo de operação:**

- Leitura simples: 5-10 segundos
- Processamento complexo: 30-60 segundos
- Operações assíncronas/batch: Não usar timeout (usar polling)

### 4. Cache de Dados Externos

**Quando cachear:**

- ✅ Dados de projetos AIDA (mudam raramente)
- ✅ Listas de referência (países, moedas, etc.)
- ✅ Configurações ou metadados

**Quando NÃO cachear:**

- ❌ Dados transacionais (solicitações, aprovações)
- ❌ Dados em tempo real (status de fluxo)
- ❌ Informações sensíveis (tokens)

**Implementação:**

- Usar `IMemoryCache` (in-process)
- TTL configurável via appsettings
- Criar serviço dedicado de cache (ex: `AidaProjetoCacheService`)

### 5. Logs e Observabilidade

**Logs obrigatórios:**

- ✅ Início da chamada (endpoint, timeout)
- ✅ Sucesso (status code, duração)
- ✅ Erro (status code, mensagem, duração)
- ✅ Retry (tentativa N de M)

**❌ NÃO logar:**

- Tokens, senhas, API keys
- Dados pessoais completos (CPF, matrícula)
- Payloads completos de request/response

**Exemplo:**

```csharp
_logger.LogInformation(
    "Chamada API externa - Endpoint: {Endpoint}, Timeout: {TimeoutMs}ms",
    endpoint, _config.TimeoutMs);

// ❌ NUNCA FAZER:
_logger.LogInformation("ApiKey: {ApiKey}", _config.ApiKey); // Expõe credencial
```

### 6. Testes Unitários

**Estratégia:**

1. **Mockar a interface de serviço** (`Mock<IAidaProjetoService>`)
2. **Testar Command Handlers** com mocks (não testar implementação HTTP)
3. **Testes de integração separados** para validar comunicação HTTP real

**Exemplo:**

```csharp
[TestMethod]
public async Task ExecuteAsync_BuscaProjeto_DeveRetornarProjeto()
{
    // ARRANGE
    var projeto = new Projeto { Codigo = "PRJ-001", Nome = "Projeto Teste" };

    _aidaServiceMock
        .Setup(x => x.BuscarProjetoPorCodigoAsync("PRJ-001", It.IsAny<CancellationToken>()))
        .ReturnsAsync(projeto);

    // ACT
    var result = await _handler.ExecuteAsync(command);

    // ASSERT
    result.IsSuccess.Should().BeTrue();
    result.Value.Codigo.Should().Be("PRJ-001");
}
```

### 7. Segurança

**Armazenamento de Credenciais:**

- ✅ AWS Secrets Manager (produção)
- ✅ Variáveis de ambiente (desenvolvimento local)
- ❌ appsettings.json (apenas placeholders)
- ❌ Hard-coded no código

**Headers de Segurança:**

```csharp
_httpClient.DefaultRequestHeaders.Add("apikey", _config.ApiKey);
_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
// Não adicionar headers sensíveis em logs
```

### 8. HttpClient - Uso Correto

**✅ FAZER:**

- Usar `HttpClientFactory` (evita esgotamento de sockets)
- Configurar timeout adequado
- Reutilizar instâncias (via DI)

**❌ NÃO FAZER:**

- `new HttpClient()` diretamente (causa vazamento de sockets)
- Timeout muito alto (> 60s sem justificativa)
- Dispor manualmente (`using var client = ...`)

### 9. Versionamento de APIs Externas

**Estratégia:**

- Incluir versão da API no `BaseUrl` (ex: `/v1/`, `/v2/`)
- Criar interfaces separadas para versões diferentes (ex: `IAidaV1Service`, `IAidaV2Service`)
- Manter adaptadores de versões antigas até migração completa

### 10. Checklist de Implementação

✅ Interface criada em `ProjSub.Aplicacao/Services/`  
✅ Interface retorna tipos do domínio (não DTOs externos)  
✅ Implementação em `ProjSub.Infraestrutura/{Sistema}/`  
✅ DTOs externos são `private` ou `internal`  
✅ Método de mapeamento Anti-Corruption implementado  
✅ Configuração via `IOptions<{Sistema}Config>`  
✅ Método `Validate()` na classe de configuração  
✅ Registro com `HttpClientFactory` no DI  
✅ Timeout configurado (padrão: 5-10s)  
✅ Tratamento de erros HTTP (404 → null, 500 → exception)  
✅ Logs estruturados (sem dados sensíveis)  
✅ Testes unitários com mocks da interface  
✅ Credenciais no Secrets Manager (não hard-coded)

---

## Referências

**Documentação Oficial:**

- [Microsoft: HttpClientFactory](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Microsoft: Options Pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [Microsoft: Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)

**Padrões e Práticas:**

- [Martin Fowler: Anti-Corruption Layer](https://martinfowler.com/bliki/AnticorruptionLayer.html)
- [Polly: Resiliência](https://github.com/App-vNext/Polly) - Biblioteca de retry/circuit breaker

**Decisões Técnicas Relacionadas:**

- [DT-005: Padrões de Estilo de Código Backend](DT-005-padroes-estilo-codigo-backend.md)
- [DT-007: Padrões de Logs e Exceções](DT-007-padroes-logs-tratamento-excecoes.md)
- [DT-008: Estratégia de Testes Backend](DT-008-estrategia-padroes-testes-backend.md)

**Arquitetura:**

- [Arquitetura a11732](../arquitetura-a11732.md) - Visão geral C4
- [Catálogo de Integrações Externas](../outros/catalogo-integracoes-externas.md) - Detalhamento de todas as APIs

---

## Notas para GitHub Copilot / Assistentes de IA

Ao gerar código de integração com APIs externas, sempre:

1. Criar interface em `ProjSub.Aplicacao/Services/I{Sistema}Service.cs`
2. Retornar tipos do domínio, nunca DTOs externos
3. Implementar Anti-Corruption Layer com método `MapearParaDominio()`
4. Usar `HttpClientFactory`, nunca `new HttpClient()`
5. Configurar timeout adequado (5-10s para leitura simples)
6. Tratar HTTP 404 como `null`, não como erro
7. Não logar credenciais, tokens ou dados sensíveis
8. Usar Options Pattern para configuração
9. Registrar com DI: `AddHttpClient<{Sistema}Service>()`
10. Criar testes com mocks da interface, não da implementação HTTP
