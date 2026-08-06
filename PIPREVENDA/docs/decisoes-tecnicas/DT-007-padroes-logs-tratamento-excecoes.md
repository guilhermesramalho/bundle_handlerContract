# DT-007: Padrões de Logs e Tratamento de Exceções

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Definir padrões obrigatórios de logging estruturado (Serilog) e tratamento de exceções para garantir observabilidade e segurança
>
> **Quando usar:** Ao configurar logging, implementar exception handling, adicionar correlação de requisições ou configurar observabilidade em produção
>
> **Palavras-chave:** `serilog` `logging` `exception-handling` `observability` `correlation-id` `structured-logging`

## Contexto

O backend PortalAle é uma aplicação distribuída que integra com múltiplos sistemas externos (ver skills `integracao-sap`/`integracao-elaw` para os contratos específicos do PIPREVENDA-1680) e serve o frontend React/Next.js. Identificamos os seguintes desafios:

1. **Logs desestruturados**: Mensagens de log em texto livre dificultam análise, busca e correlação entre requisições
2. **Exposição de detalhes técnicos**: Stack traces e mensagens de erro internas são expostas ao frontend, representando risco de segurança
3. **Dificuldade de rastreamento**: Sem identificador de correlação, é difícil rastrear uma requisição através de múltiplos serviços
4. **Tratamento inconsistente**: Alguns endpoints lançam exceções para fluxos de negócio, outros usam Result Pattern, gerando inconsistência
5. **Dados sensíveis em logs**: Risco de vazar senhas, tokens e informações pessoais nos logs
6. **Observabilidade limitada**: Falta de contexto estruturado nos logs dificulta troubleshooting em produção

**Necessidade**: Definir padrões obrigatórios para logging e exception handling que garantam observabilidade, segurança e consistência em toda a aplicação.

---

## Decisão

Adotar os seguintes padrões para logs e tratamento de exceções no backend PortalAle:

### 1. Logs Estruturados com Serilog

Usar **Serilog** como biblioteca de logging, com logs estruturados contendo propriedades específicas para facilitar busca e análise.

**Configuração obrigatória:**

```csharp
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("Aplicacao", "PortalAle.Api")
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.File(
            new JsonFormatter(),
            "logs/portalale-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30);
});
```

**Propriedades obrigatórias em logs de requisições:**

- `CorrelationId` - Identificador único da requisição (propagado via header `X-Correlation-ID`)
- `UserId` - Login do usuário autenticado
- `UserName` - Nome completo do usuário
- `Endpoint` - Rota HTTP acessada
- `Method` - Verbo HTTP (GET, POST, etc.)
- `StatusCode` - Status HTTP da resposta
- `Duration` - Duração da requisição em milissegundos

---

### 2. Níveis de Log

Usar níveis de log de forma consistente:

| Nível           | Quando Usar                                         | Exemplos                                                                       |
| --------------- | --------------------------------------------------- | ------------------------------------------------------------------------------ |
| **Trace**       | Detalhes extremamente verbosos para debugging local | Valores de variáveis em loops, estados intermediários                          |
| **Debug**       | Informações de debugging úteis em desenvolvimento   | SQL gerado pelo EF Core, payloads de requisições externas                      |
| **Information** | Eventos de fluxo normal da aplicação                | Usuário autenticado, projeto criado, integração com AIDA bem-sucedida          |
| **Warning**     | Situações anômalas que não impedem o funcionamento  | API externa lenta (> 5s), cache miss, retry bem-sucedido                       |
| **Error**       | Falhas que impactam a requisição atual              | Falha ao chamar API externa, erro de validação inesperado, timeout de banco    |
| **Critical**    | Falhas graves que tornam a aplicação indisponível   | Banco de dados inacessível, falha ao iniciar aplicação, perda de conectividade |

**Regras:**

- ✅ **Production**: Mínimo `Information` (nunca `Debug` ou `Trace`)
- ✅ **Staging**: Mínimo `Debug`
- ✅ **Development**: Mínimo `Trace`
- ❌ **NUNCA logar**: Validações de campo ou regras de negócio que retornam Result.Failure (poluem logs com ruído)
- ✅ **Logar apenas**: Operações bem-sucedidas (Information) e erros de infraestrutura/inesperados (Error/Critical)

---

### 3. Tratamento de Exceções

### 4. Segurança em Logs

#### 4.1. Dados Sensíveis Proibidos

**Nunca registrar:**

- ❌ Senhas (plain text, hash ou qualquer formato)
- ❌ Tokens de autenticação (JWT, API keys, refresh tokens)
- ❌ Dados de cartão de crédito
- ❌ CPF completo (mascarar: `123.456.789-**`)
- ❌ Dados pessoais sensíveis (PII - Personally Identifiable Information)

**Dados permitidos em logs:**

- ✅ Login do usuário (matrícula)
- ✅ Nome completo do usuário
- ✅ IDs de recursos (projeto ID, avanço ID)
- ✅ Status de operações (sucesso, falha)
- ✅ Metadados de requisição (endpoint, método, duração)

---

### 5. Correlação e Rastreabilidade

#### 5.1. Correlation ID

Implementar middleware para propagar identificador de correlação:

```csharp
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

**Regras:**

- ✅ Frontend deve enviar `X-Correlation-ID` em todas as requisições
- ✅ Backend deve propagar o mesmo ID para todas as integrações externas
- ✅ Backend deve retornar o `X-Correlation-ID` no header de resposta
- ✅ Todos os logs devem incluir o `CorrelationId`

---

#### 5.2. Logging de Requisições HTTP

> #TODO: Revisar regra para somente registrar requisição caso esteja em modo debug

Registrar todas as requisições HTTP com contexto completo:

```csharp
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var sw = Stopwatch.StartNew();

    logger.LogInformation(
        "Requisição iniciada: {Method} {Path} por {UserId}",
        context.Request.Method,
        context.Request.Path,
        context.User.Identity?.Name ?? "Anônimo");

    await next();

    sw.Stop();

    logger.LogInformation(
        "Requisição finalizada: {Method} {Path} - Status {StatusCode} - Duração {Duration}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        sw.ElapsedMilliseconds);
});
```

---

## Alternativas Consideradas

### 1. Log4Net ou NLog ao invés de Serilog

**Descrição**: Usar Log4Net ou NLog como biblioteca de logging.

**Vantagens:**

- Bibliotecas maduras e estáveis
- Grande comunidade e documentação extensa

**Desvantagens:**

- Menos suporte para logs estruturados nativos
- Configuração mais verbosa
- Menos integração com ferramentas modernas de observabilidade (Seq, Elastic)

**Por que foi rejeitada**: Serilog oferece melhor suporte para logs estruturados, configuração via código mais simples e melhor integração com .NET moderno e ferramentas de observabilidade.

---

## Consequências

### Positivas

- ✅ **Observabilidade**: Logs estruturados facilitam busca, análise e troubleshooting
- ✅ **Rastreabilidade**: CorrelationId permite rastrear requisições end-to-end
- ✅ **Segurança**: Dados sensíveis não são expostos em logs
- ✅ **Consistência**: Padrões claros eliminam ambiguidade sobre como logar e tratar erros
- ✅ **Debugging**: Stack traces completos em logs facilitam diagnóstico

### Negativas

- ⚠️ **Boilerplate**: Middleware e configurações adicionam código de infraestrutura
- ⚠️ **Disciplina**: Requer que toda a equipe siga os padrões consistentemente

### Riscos Mitigados

- ✅ **Vazamento de dados sensíveis**: Mascaramento e proibição de certos dados em logs
- ✅ **Logs inutilizáveis**: Estruturação garante que logs possam ser pesquisados e analisados
- ✅ **Exposição de detalhes internos**: Middleware garante que stack traces nunca chegam ao frontend

---

## Implementação

### 3. Configuração em appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

**Por ambiente:**

- `appsettings.Development.json`: `"Default": "Debug"`
- `appsettings.Staging.json`: `"Default": "Debug"`
- `appsettings.Production.json`: `"Default": "Information"`

---

## Exemplos de Uso

### Exemplo 1: Log Estruturado com Contexto

```csharp
_logger.LogInformation(
    "Projeto {ProjetoId} criado por {UserId} com sucesso. Nome: {ProjetoNome}",
    projeto.Id,
    usuario.Login,
    projeto.Nome);
```

**Output JSON:**

```json
{
  "Timestamp": "2026-02-01T10:30:00.123Z",
  "Level": "Information",
  "MessageTemplate": "Projeto {ProjetoId} criado por {UserId} com sucesso. Nome: {ProjetoNome}",
  "CorrelationId": "abc123-def456-ghi789",
  "ProjetoId": 42,
  "UserId": "f0q3",
  "ProjetoNome": "Novo Poço P-99",
  "Aplicacao": "PortalAle.Api",
  "MachineName": "portalale-api-001",
  "EnvironmentName": "Production"
}
```

---

### Exemplo 2: Mascaramento de Dados Sensíveis

```csharp
// ❌ Incorreto - logar token completo
_logger.LogInformation("Token obtido: {Token}", token);

// ✅ Correto - mascarar token
_logger.LogInformation("Token obtido: {Token}", token.MaskToken());

// ❌ Incorreto - logar CPF completo
_logger.LogInformation("CPF do usuário: {Cpf}", usuario.Cpf);

// ✅ Correto - mascarar CPF
_logger.LogInformation("CPF do usuário: {Cpf}", usuario.Cpf.MaskCpf());
```

---

## Referências

- **Serilog Documentation**: https://serilog.net/
- **Structured Logging Best Practices**: https://github.com/serilog/serilog/wiki/Structured-Data
- **Microsoft Logging Guidelines**: https://learn.microsoft.com/en-us/dotnet/core/extensions/logging
- **OWASP Logging Cheat Sheet**: https://cheatsheetseries.owasp.org/cheatsheets/Logging_Cheat_Sheet.html
- **DT-006: Padrão de Implementação de API REST**: Tratamento de erros HTTP
