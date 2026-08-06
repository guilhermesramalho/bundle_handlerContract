# DT-005: Padrões de Estilo de Código e Qualidade - Backend

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Estabelecer convenções de código C# e ferramentas de qualidade obrigatórias para garantir consistência e manutenibilidade do backend
>
> **Quando usar:** Ao configurar novo repositório, revisar padrões de codificação ou configurar ferramentas de análise estática (EditorConfig, StyleCop, SonarQube)
>
> **Palavras-chave:** `csharp` `dotnet` `coding-style` `editorconfig` `stylecop` `sonarqube`

## Contexto

O projeto backend PortalAle (PIPREVENDA-1680) é desenvolvido em **C# / .NET 9** com múltiplos desenvolvedores contribuindo simultaneamente. Sem convenções de código claras e ferramentas de aplicação automática, o código tende a apresentar:

- **Inconsistência** de nomenclatura (PascalCase vs camelCase)
- **Estilos de formatação** divergentes (indentação, chaves, espaçamento)
- **Baixa legibilidade** por falta de padrões de estruturação
- **Dificuldade de manutenção** ao misturar idiomas (português/inglês)
- **Retrabalho em code reviews** para corrigir violações de estilo

A necessidade é estabelecer **convenções oficiais e consistentes** para todo o código backend, com ferramentas que automatizem a aplicação dessas regras.

---

## Decisão

**Adotar as convenções oficiais de código C# da Microsoft** como padrão para o projeto backend, com aplicação automática via ferramentas integradas ao fluxo de desenvolvimento.

### Convenções de Nomenclatura

Seguir as regras documentadas em:

- **[C# Identifier Naming Rules and Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)**
- **[C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)**

**Padrões adotados:**

| Elemento                                      | Convenção                              | Exemplo                                          |
| --------------------------------------------- | -------------------------------------- | ------------------------------------------------ |
| Classes, Interfaces, Structs, Records         | **PascalCase**                         | `DataService`, `IWorkerQueue`, `PhysicalAddress` |
| Interfaces                                    | **PascalCase com prefixo `I`**         | `IWorkerQueue`, `IRepository`                    |
| Métodos e Propriedades Públicas               | **PascalCase**                         | `StartEventProcessing()`, `WorkerQueue`          |
| Parâmetros e Variáveis Locais                 | **camelCase**                          | `someNumber`, `isValid`, `customerName`          |
| Campos Privados                               | **\_camelCase** (underscore prefixado) | `_workerQueue`, `_logger`                        |
| Primary Constructor Parameters (class/struct) | **camelCase**                          | `firstName`, `age`                               |
| Primary Constructor Parameters (record)       | **PascalCase**                         | `FirstName`, `Age` (são propriedades públicas)   |
| Constantes                                    | **PascalCase**                         | `MaxRetryAttempts`, `DefaultTimeout`             |
| Campos Static Private                         | **s_camelCase**                        | `s_workerQueue`                                  |
| Campos Thread Static                          | **t_camelCase**                        | `t_timeSpan`                                     |

### Convenções de Formatação

| Aspecto               | Padrão                                                          |
| --------------------- | --------------------------------------------------------------- |
| Indentação            | **4 espaços** para C#, **2 espaços** para JSON/XML              |
| Encoding              | **UTF-8 com LF** (Unix line endings)                            |
| Estilo de chaves      | **Allman style** (chave de abertura e fechamento em nova linha) |
| Declarações por linha | **Uma declaração por linha**, **um statement por linha**        |
| Namespace             | **File-scoped** (`namespace MySampleCode;`)                     |
| Using directives      | **Fora da declaração de namespace**                             |

### Convenções de Linguagem

| Aspecto                | Regra                                                                                        |
| ---------------------- | -------------------------------------------------------------------------------------------- |
| Código-fonte           | **Português** (classes, métodos, variáveis), principalmente na camada de aplicação e domínio |
| Comentários e XML docs | **Português**, termos técnicos em inglês quando sem tradução adequada                        |
| Mensagens de erro      | **Português**                                                                                |
| Tipos primitivos       | Usar **keywords** (`string`, `int`) ao invés de tipos .NET (`String`, `Int32`)               |
| Uso de `var`           | Apenas quando **tipo é óbvio** pelo lado direito da atribuição                               |
| String concatenation   | Preferir **string interpolation** (`$"texto {variavel}"`)                                    |
| Coleções               | Usar **collection expressions** para inicialização                                           |

## Alternativas Consideradas

### 1. Criar Convenções Customizadas Próprias

**Rejeitada** - Reinventar convenções diverge do ecossistema .NET e dificulta onboarding de novos desenvolvedores familiarizados com padrões oficiais Microsoft.

### 2. Não Enforçar Automaticamente (Code Review Manual)

**Rejeitada** - Code reviews manuais para estilo de código são ineficientes, consomem tempo da equipe e são inconsistentes. Ferramentas automatizam 95% das verificações.

---

## Consequências

### Positivas ✅

1. **Consistência de Código** - Todo código segue o mesmo padrão, independente do autor
2. **Onboarding Facilitado** - Novos desenvolvedores já conhecem convenções Microsoft
3. **Qualidade Aumentada** - Analyzers detectam code smells, bugs potenciais e vulnerabilidades
4. **Compatibilidade com IDEs** - EditorConfig funciona em Visual Studio, VS Code, Rider
5. **Alinhamento com Ecossistema .NET** - Facilita uso de bibliotecas e ferramentas de terceiros

### Negativas ⚠️

1. **Configuração de Ambiente** - Requer instalação de extensões (EditorConfig, C# DevKit, SonarLint)
2. **Warnings em Código Legado** - Código existente pode gerar muitos avisos (aceitável, corrigir gradualmente)

---

## Implementação

### Passo 1: Configuração de Ambiente (Todos os Desenvolvedores)

**Instalar extensões obrigatórias:**

```bash
# VS Code
code --install-extension editorconfig.editorconfig
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension sonarsource.sonarlint-vscode
```

**Ativar Git Hooks:**

```powershell
git config core.hooksPath .husky
```

### Passo 2: Formatação Manual (Quando Necessário)

```powershell
# Formatar todo o código
.\scripts\format-code.ps1

# Ou usando dotnet CLI
dotnet format GestaoContratoAle.sln
```

### Passo 3: Análise Local (Antes de Push)

```powershell
# Build com análise
dotnet build GestaoContratoAle.sln /p:EnforceCodeStyleInBuild=true

# Análise SonarQube local
.\scripts\run-sonar-analysis.ps1 -ServerUrl "https://sonar.petrobras.com.br" -Token "seu-token"
```

### Passo 4: Fluxo de Commit

1. Desenvolver feature/bugfix normalmente
2. **Pre-commit hook** executa automaticamente:
   - `dotnet format` nos arquivos modificados
   - Build rápido para verificar compilação
3. Se hooks passarem → Commit criado
4. **Pre-push hook** executa:
   - Sincronização submódulo docs
   - Build completo com análise de código
5. Se hooks passarem → Push enviado

---

### Dashboards

- **SonarQube** - https://sonar.petrobras.com.br/dashboard?id=a11732-backend

---

## Referências

### Documentação Oficial Microsoft

- **[C# Identifier Naming Rules and Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)** - Regras de nomenclatura
- **[C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)** - Convenções gerais de código

### Ferramentas

- **[EditorConfig](https://editorconfig.org/)** - Configuração cross-IDE
- **[Roslynator Analyzers](https://github.com/dotnet/roslynator)** - 500+ regras de análise
- **[.NET Code Analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)** - Analyzers oficiais
- **[SonarQube .NET](https://docs.sonarqube.org/latest/analysis/languages/csharp/)** - Análise de qualidade
