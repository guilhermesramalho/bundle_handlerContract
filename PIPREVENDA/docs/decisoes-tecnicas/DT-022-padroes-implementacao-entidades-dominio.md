# DT-022: Padrões de Implementação de Entidades de Domínio

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Estabelecer padrões de implementação para entidades de domínio seguindo Domain-Driven Design (DDD) e Clean Architecture, garantindo consistência, testabilidade e manutenibilidade do modelo de domínio
>
> **Quando usar:** Ao criar novas entidades de domínio, modificar entidades existentes, implementar agregados, value objects, ou revisar padrões de modelagem do domínio
>
> **Palavras-chave:** `domain-driven-design` `ddd` `clean-architecture` `entity` `aggregate` `value-object` `domain-model` `invariants`

---

## Contexto

O projeto backend +Digital A11732 é desenvolvido seguindo os princípios de **Clean Architecture** e **Domain-Driven Design (DDD)**. A camada de domínio (`ProjSub.Dominio`) é o núcleo da aplicação, contendo as regras de negócio e o modelo conceitual do sistema.

### Problemas Identificados sem Padrões Claros

Sem convenções estabelecidas para implementação de entidades de domínio, observamos:

1. **Inconsistência Estrutural**
   - Algumas entidades validam estado em construtores, outras em métodos separados
   - Entidades podem entrar em estado inválido temporariamente
   - Falta padronização entre construtores ricos, factory methods e setters públicos

2. **Acoplamento Inadequado com Infraestrutura**
   - Relacionamentos modelados apenas com FKs (sem navigation properties) ou vice-versa
   - Entity Framework não consegue mapear relacionamentos corretamente
   - Referências circulares e problemas de lazy loading

3. **Falta de Garantia de Invariantes**
   - Entidades com setters públicos permitem bypass de validações
   - Métodos `ValidarRegrasDeNegocio()` chamados manualmente podem ser esquecidos
   - Estado inconsistente durante construção ou mutação

4. **Nomenclatura Inconsistente**
   - Mistura de português e inglês no modelo de domínio
   - FKs nomeadas diferentemente (`ProjetoId`, `IdProjeto`, `projeto_id`)
   - Navigation properties sem padrão claro

5. **Baixa Testabilidade**
   - Entidades com lógica de negócio não testada
   - Falta de testes unitários para validações e invariantes
   - Dificuldade de instanciar entidades para testes

### Necessidade

Estabelecer **padrões claros e consistentes** para implementação de entidades de domínio que:

- Garantam invariantes do domínio (entidades sempre em estado válido)
- Facilitem mapeamento objeto-relacional com Entity Framework
- Promovam alta testabilidade e manutenibilidade
- Alinhem-se com os princípios DDD e Clean Architecture
- Mantenham nomenclatura em português para o domínio de negócio

---

## Decisão

**Adotar padrões de implementação de entidades de domínio baseados em Domain-Driven Design**, com as seguintes diretrizes:

### 1. Estrutura Base de Entidades

**Todas as entidades devem herdar de `Entity`** (classe base que fornece identidade):

```csharp
public class Entity
{
    public int Id { get; set; }

    public Entity() { }
    public Entity(int id) { Id = id; }

    public override bool Equals(object? obj) { /* equality by Id */ }
    public override int GetHashCode() { /* hash by Id */ }
}
```

**Toda entidade deve ter três construtores:**

1. **Parameterless**: para Entity Framework Core (pode ser `protected` ou `private`)
2. **Com Id**: para testes (`public Projeto(int id) : base(id)`)
3. **Rico com parâmetros**: para criação válida com invariantes garantidas

### 2. Garantia de Invariantes

**Princípio fundamental: Entidade NUNCA deve entrar em estado inválido.**

- **Validações ocorrem ANTES de atribuições** (construtores e métodos de mutação)
- **Lançar `DomainException` imediatamente** se dados inválidos
- **NÃO criar método `ValidarRegrasDeNegocio()` separado** (validação após o fato)

```csharp
public class Projeto : Entity
{
    public Projeto(string nome, string descricao, int faseId)
    {
        // ✅ Validações ANTES de atribuições
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do projeto é obrigatório.");

        if (nome.Length > 100)
            throw new DomainException("Nome não pode ter mais de 100 caracteres.");

        // Atribuições após validação
        Nome = nome.Trim();
        Descricao = descricao.Trim();
        FaseId = faseId;
        DataCriacao = DateTime.UtcNow;
    }
}
```

### 3. Relacionamentos: FK + Navigation Properties

**Para cada relacionamento, são OBRIGATÓRIAS duas propriedades:**

1. **FK Property**: `{Entidade}Id` (ex: `ProjetoId`, `StatusId`)
2. **Navigation Property**: nome da entidade (ex: `Projeto`, `Status`)

Isso permite que Entity Framework mapeie corretamente o relacionamento.

```csharp
// ✅ CORRETO - Ambas propriedades presentes
public class Risco : Entity
{
    public int ProjetoId { get; set; }              // FK
    public virtual Projeto? Projeto { get; set; }   // Navigation
}

// ❌ INCORRETO - Apenas FK (EF não mapeia navigation automaticamente)
public class Risco : Entity
{
    public int ProjetoId { get; set; }
}

// ❌ INCORRETO - Apenas Navigation (EF cria shadow FK, dificulta queries)
public class Risco : Entity
{
    public virtual Projeto? Projeto { get; set; }
}
```

### 4. Nomenclatura em Português

- **Entidades**: substantivo singular (`Projeto`, `Risco`, `Acao`, `Contratacao`)
- **Propriedades**: português claro (`Nome`, `Descricao`, `DataInicio`)
- **FKs**: `{Entidade}Id` (`ProjetoId`, `RiscoId`, `StatusId`)
- **Collections**: plural (`Riscos`, `Acoes`, `Responsaveis`)
- **Termos técnicos consolidados**: podem permanecer em inglês (`Repository`, `Handler`)

### 5. Construção de Entidades Complexas

Para entidades com muitos parâmetros, três opções:

#### Opção A: Construtor Rico

```csharp
public Projeto(string nome, string descricao, int faseId, int statusId, string gerencia)
{
    // Validações + atribuições
}
```

#### Opção B: Factory Method Estático

```csharp
public static Contratacao Criar(string descricao, StatusContratacao status, string pji)
{
    var contratacao = new Contratacao();
    // Validações + atribuições
    return contratacao;
}
```

#### Opção C: Builder Pattern (casos muito complexos)

```csharp
var projeto = new ProjetoBuilder()
    .ComNome("Projeto X")
    .ComDescricao("...")
    .ComFase(faseId)
    .Construir(); // Valida e retorna Projeto
```

#### Opção D: Setters Públicos com Validação

Para propriedades que precisam ser alteradas frequentemente ou quando validação isolada por propriedade é suficiente:

```csharp
public class Projeto : Entity
{
    private string _nome = string.Empty;
    public string Nome
    {
        get => _nome;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Nome do projeto é obrigatório.");

            if (value.Length > 100)
                throw new DomainException("Nome não pode ter mais de 100 caracteres.");

            _nome = value.Trim();
        }
    }

    // Construtores parameterless e com id ainda necessários para EF e testes
    public Projeto() { }
    public Projeto(int id) : base(id) { }
}
```

**Quando usar:**

- ✅ Validações independentes por propriedade
- ✅ Propriedades que mudam frequentemente
- ✅ Integração com frameworks que exigem setters públicos

**Atenção:**

- ⚠️ Dificulta validação de invariantes entre múltiplas propriedades
- ⚠️ Pode ter problemas com EF Core se setter lançar exceção durante materialização
- ⚠️ Combine com construtor rico para garantir estado inicial válido

### 6. Value Objects

Para conceitos sem identidade própria (endereço, dinheiro, email):

```csharp
public record Endereco
{
    public string Logradouro { get; }  // Imutável
    public string Cidade { get; }

    public Endereco(string logradouro, string cidade)
    {
        // Validações
        Logradouro = logradouro.Trim();
        Cidade = cidade.Trim();
    }
}
```

### 7. Exceções de Domínio

Usar `DomainException` genérica com mensagens claras:

```csharp
// ✅ Preferido
throw new DomainException("Nome do projeto é obrigatório.");

// ✅ Múltiplos erros
throw new DomainException(new List<string> {
    "Nome é obrigatório.",
    "Data de início não pode ser no passado."
});

// ✅ Exceção específica (se necessário, mas deve herdar de DomainException)
public class ProjetoValidationException : DomainException
{
    public ProjetoValidationException(string message) : base(message) { }
}
```

### 8. Testes Unitários Obrigatórios

**Toda regra de negócio em entidade DEVE ter teste unitário** em `ProjSub.DominioTestes`:

```csharp
public class ProjetoTests
{
    [Fact]
    public void Criar_ComNomeVazio_DeveLancarException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Projeto("", "Descrição", faseId: 1));

        Assert.Contains("Nome do projeto é obrigatório", ex.Message);
    }

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarProjetoCorretamente()
    {
        var projeto = new Projeto("Projeto X", "Descrição", faseId: 1);

        Assert.Equal("Projeto X", projeto.Nome);
        Assert.True(projeto.Ativo);
        Assert.NotEqual(default, projeto.DataCriacao);
    }
}
```

---

## Alternativas Consideradas

### 1. Modelo Anêmico (Anemic Domain Model)

**Descrição**: Entidades são DTOs sem comportamento, toda lógica em serviços.

**Exemplo**:

```csharp
public class Projeto
{
    public string Nome { get; set; }  // Setter público
    public string Descricao { get; set; }
}

// Lógica em serviço
public class ProjetoService
{
    public void CriarProjeto(Projeto projeto)
    {
        if (string.IsNullOrWhiteSpace(projeto.Nome))
            throw new Exception("Nome inválido");
    }
}
```

**Rejeitado porque**:

- ❌ Viola encapsulamento (setters públicos)
- ❌ Entidade pode entrar em estado inválido
- ❌ Lógica de domínio espalhada em serviços
- ❌ Baixa coesão e difícil manutenção
- ❌ Contraria princípios DDD

### 2. Validação Posterior com Método `Validar()`

**Descrição**: Entidade criada/alterada primeiro, validada depois.

**Exemplo**:

```csharp
public class Projeto : Entity
{
    public string Nome { get; set; }

    public void Validar()
    {
        if (string.IsNullOrWhiteSpace(Nome))
            throw new Exception("Nome inválido");
    }
}

// Uso
var projeto = new Projeto { Nome = "" };
projeto.Validar(); // Pode ser esquecido!
```

**Rejeitado porque**:

- ❌ Entidade pode existir em estado inválido temporariamente
- ❌ Desenvolvedor pode esquecer de chamar `Validar()`
- ❌ Não garante invariantes durante todo ciclo de vida
- ❌ Inconsistente com princípios DDD (sempre-válido)

### 3. Apenas Navigation Properties (Sem FKs Explícitas)

**Descrição**: Relacionamentos apenas com navigation properties, EF cria shadow properties.

**Exemplo**:

```csharp
public class Risco : Entity
{
    public virtual Projeto? Projeto { get; set; }
    // EF cria "ProjetoId" shadow property
}
```

**Rejeitado porque**:

- ❌ Shadow properties dificultam queries (não aparecem em LINQ)
- ❌ Impossível fazer `.Where(r => r.ProjetoId == x)` sem incluir navigation
- ❌ Complicada configuração de índices e FKs no mapeamento
- ❌ Performance inferior (lazy loading forçado)
- ❌ Dificulta testes unitários (necessário mockar navigation)

---

## Consequências

### Positivas ✅

1. **Invariantes Garantidas**
   - Entidades sempre em estado válido
   - Impossível criar ou modificar entidade com dados inválidos
   - Reduz bugs relacionados a estado inconsistente

2. **Alta Testabilidade**
   - Entidades testáveis sem infraestrutura
   - Testes unitários simples e rápidos
   - Facilita TDD (Test-Driven Development)

3. **Encapsulamento Adequado**
   - Lógica de negócio dentro da entidade
   - Setters privados protegem estado
   - Mutação controlada por métodos de negócio

4. **Mapeamento ORM Correto**
   - FK + Navigation Properties permitem mapeamento completo
   - Entity Framework configura relacionamentos automaticamente
   - Queries LINQ eficientes com FKs

5. **Documentação Viva**
   - Código expressa regras de negócio claramente
   - Nomes em português facilitam compreensão
   - Invariantes explícitas em construtores

6. **Manutenibilidade**
   - Padrão consistente em todas entidades
   - Fácil localizar validações (sempre em construtores/métodos)
   - Refatoração segura (testes unitários cobrem regras)

### Negativas ⚠️

1. **Curva de Aprendizado**
   - Desenvolvedores acostumados com modelo anêmico precisam se adaptar
   - Requer compreensão de DDD e construtores ricos
   - Necessário treinar equipe em Value Objects

2. **Verbosidade Inicial**
   - Construtores com muitos parâmetros podem ser longos
   - Necessário criar factory methods ou builders para casos complexos
   - Mais código inicial comparado a setters públicos simples

3. **Dupla Propriedade para Relacionamentos**
   - FK + Navigation aumenta número de propriedades
   - Necessário manter sincronização (EF faz automaticamente, mas é abstração adicional)

4. **Migração de Código Legado**
   - Entidades existentes precisam ser refatoradas
   - Pode gerar muitos testes para garantir comportamento preservado
   - Migrations do EF podem ser necessárias se estrutura mudar

### Riscos Mitigados 🛡️

- **Estado Inválido**: Eliminado por design (validação antes de atribuição)
- **Bugs de Validação**: Reduzidos (impossível esquecer validação)
- **Problemas de Mapeamento ORM**: Eliminados (FK + Navigation completos)
- **Testes Frágeis**: Reduzidos (entidades isoladas, sem dependências)
- **Código Espaguete**: Mitigado (lógica no domínio, não em serviços)

---

## Implementação

### Passo 1: Estrutura Base

Garantir que `ProjSub.Dominio/Base/Entity.cs` exista:

```csharp
public class Entity
{
    public int Id { get; set; }
    public Entity() { }
    public Entity(int id) { Id = id; }
    public override bool Equals(object? obj) { /* ... */ }
    public override int GetHashCode() { /* ... */ }
}
```

### Passo 2: Criar Entidade com Template

```csharp
using ProjSub.Dominio.Base;
using ProjSub.Dominio.Exceptions;

namespace ProjSub.Dominio;

public class NomeEntidade : Entity
{
    // 1. Propriedades primitivas
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;

    // 2. FKs + Navigation properties
    public int ReferenciaId { get; set; }
    public virtual EntidadeReferencia? Referencia { get; set; }

    // 3. Collections
    public virtual ICollection<EntidadeFilha> Filhas { get; private set; } = new List<EntidadeFilha>();

    // 4. Construtores
    public NomeEntidade() { }  // EF Core
    public NomeEntidade(int id) : base(id) { }  // Testes

    // 5. Construtor rico com validação
    public NomeEntidade(string nome, string descricao, int referenciaId)
    {
        ValidarNome(nome);
        ValidarDescricao(descricao);

        Nome = nome.Trim();
        Descricao = descricao.Trim();
        ReferenciaId = referenciaId;
        DataCriacao = DateTime.UtcNow;
    }

    // 6. Métodos de validação privados
    private void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");

        if (nome.Length > 100)
            throw new DomainException("Nome não pode ter mais de 100 caracteres.");
    }

    private void ValidarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória.");
    }

    // 7. Métodos de mutação com validação
    public void AtualizarNome(string novoNome)
    {
        ValidarNome(novoNome);
        Nome = novoNome.Trim();
        DataAtualizacao = DateTime.UtcNow;
    }
}
```

### Passo 3: Criar Testes Unitários

```csharp
// ProjSub.DominioTestes/NomeEntidadeTests.cs
using Xunit;
using ProjSub.Dominio;
using ProjSub.Dominio.Exceptions;

public class NomeEntidadeTests
{
    [Fact]
    public void Criar_ComNomeVazio_DeveLancarException()
    {
        var ex = Assert.Throws<DomainException>(() =>
            new NomeEntidade("", "Descrição", referenciaId: 1));

        Assert.Contains("Nome é obrigatório", ex.Message);
    }

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarCorretamente()
    {
        var entidade = new NomeEntidade("Nome", "Descrição", referenciaId: 1);

        Assert.Equal("Nome", entidade.Nome);
        Assert.Equal("Descrição", entidade.Descricao);
        Assert.NotEqual(default, entidade.DataCriacao);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComNomeInvalido_DeveLancarException(string nomeInvalido)
    {
        Assert.Throws<DomainException>(() =>
            new NomeEntidade(nomeInvalido, "Descrição", referenciaId: 1));
    }
}
```

### Passo 4: Value Objects (Quando Aplicável)

```csharp
public class Email
{
    public string Endereco { get; }

    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new DomainException("Email é obrigatório.");

        if (!endereco.Contains("@"))
            throw new DomainException("Email inválido.");

        Endereco = endereco.ToLowerInvariant().Trim();
    }

    public override bool Equals(object? obj)
    {
        return obj is Email other && Endereco == other.Endereco;
    }

    public override int GetHashCode() => Endereco.GetHashCode();

    public override string ToString() => Endereco;
}

// Uso em entidade
public class Usuario : Entity
{
    public Email Email { get; private set; }

    public Usuario(string nome, Email email)
    {
        Nome = nome;
        Email = email ?? throw new DomainException("Email é obrigatório.");
    }
}
```

### Passo 5: Configuração EF Core

No mapeamento (`IEntityTypeConfiguration<T>`), configurar FK + Navigation:

```csharp
public class RiscoConfiguration : IEntityTypeConfiguration<Risco>
{
    public void Configure(EntityTypeBuilder<Risco> builder)
    {
        // FK
        builder.Property(r => r.ProjetoId)
            .HasColumnName("risc_cd_projeto")
            .HasColumnType("integer")
            .IsRequired();

        // Relacionamento
        builder.HasOne(r => r.Projeto)
            .WithMany(p => p.Riscos)
            .HasForeignKey(r => r.ProjetoId)
            .HasConstraintName("fk_proj_risc")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

---

## Verificação de Conformidade

Use este checklist para auditar se o padrão está sendo seguido:

### Estrutura Base

- [ ] Entidade herda de `Entity`
- [ ] Possui construtor parameterless (para EF Core)
- [ ] Possui construtor com `int id` (para testes)
- [ ] Possui construtor rico OU método estático de fábrica
- [ ] Nomes em português claro e sem ambiguidade

### Relacionamentos

- [ ] Para CADA relacionamento: FK property (`{Entidade}Id`) + navigation property (`{Entidade}`)
- [ ] Collections inicializadas com `new List<>()`
- [ ] Collections com `private set` quando apropriado
- [ ] `virtual` usado apenas se lazy loading necessário

### Invariantes e Validações

- [ ] **Validações ANTES de atribuições** (nunca estado inválido)
- [ ] Métodos de mutação validam antes de alterar estado
- [ ] Validações lançam `DomainException` com mensagens claras em português
- [ ] **NÃO existe método** `ValidarRegrasDeNegocio()` chamado separadamente
- [ ] Setters são `private set` quando não devem ser alterados externamente

### Value Objects

- [ ] Conceitos sem identidade própria implementados como Value Objects
- [ ] Value Objects são imutáveis (propriedades com `{ get; }`)
- [ ] Value Objects implementam igualdade por valor (`Equals`, `GetHashCode`)
- [ ] Validações no construtor do Value Object

### Testes Unitários

- [ ] Toda regra de negócio tem teste unitário em `ProjSub.DominioTestes`
- [ ] Testes para construtores (casos válidos e inválidos)
- [ ] Testes para métodos de mutação
- [ ] Testes verificam lançamento de `DomainException` com mensagem correta
- [ ] Testes verificam estado da entidade após operações

### Qualidade de Código

- [ ] Sem lógica de persistência/infraestrutura na entidade
- [ ] Sem dependências de outras camadas (Aplicação, Infraestrutura)
- [ ] Encapsulamento adequado (behavior over data)
- [ ] Métodos de negócio expressam linguagem ubíqua do domínio
- [ ] Código segue padrões de estilo C# (DT-005)

### Documentação

- [ ] Comentários XML apenas quando necessário (código deve ser autoexplicativo)
- [ ] Regras de negócio complexas documentadas
- [ ] Invariantes importantes documentadas no construtor

---

## Exemplos Completos

### Exemplo 1: Entidade Simples (Lookup)

```csharp
public class StatusProjeto : Entity
{
    public string Nome { get; private set; } = string.Empty;
    public string Cor { get; private set; } = string.Empty;
    public int Ordem { get; private set; }
    public bool Ativo { get; private set; } = true;

    public virtual ICollection<Projeto> Projetos { get; private set; } = new List<Projeto>();

    public StatusProjeto() { }
    public StatusProjeto(int id) : base(id) { }

    public StatusProjeto(string nome, string cor, int ordem)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do status é obrigatório.");

        Nome = nome.Trim();
        Cor = cor.Trim();
        Ordem = ordem;
    }

    public void Desativar()
    {
        if (Projetos.Any(p => p.Ativo))
            throw new DomainException("Não é possível desativar status com projetos ativos.");

        Ativo = false;
    }
}
```

### Exemplo 2: Entidade com Relacionamentos

```csharp
public class Risco : Entity
{
    // Propriedades
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    // FK + Navigation (obrigatórias)
    public int ProjetoId { get; set; }
    public virtual Projeto? Projeto { get; set; }

    public int TipoRiscoId { get; set; }
    public virtual TipoRisco? TipoRisco { get; set; }

    public string ResponsavelChave { get; private set; } = string.Empty;

    // Collection
    public virtual ICollection<Acao> Acoes { get; private set; } = new List<Acao>();

    // Construtores
    public Risco() { }
    public Risco(int id) : base(id) { }

    public Risco(string titulo, string descricao, int projetoId, int tipoRiscoId, string responsavelChave)
    {
        ValidarTitulo(titulo);
        ValidarDescricao(descricao);
        ValidarResponsavel(responsavelChave);

        Titulo = titulo.Trim();
        Descricao = descricao.Trim();
        ProjetoId = projetoId;
        TipoRiscoId = tipoRiscoId;
        ResponsavelChave = responsavelChave.Trim();
    }

    // Validações
    private void ValidarTitulo(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("Título do risco é obrigatório.");

        if (titulo.Length > 200)
            throw new DomainException("Título não pode ter mais de 200 caracteres.");
    }

    private void ValidarDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição do risco é obrigatória.");
    }

    private void ValidarResponsavel(string responsavel)
    {
        if (string.IsNullOrWhiteSpace(responsavel))
            throw new DomainException("Responsável é obrigatório.");
    }

    // Métodos de negócio
    public void AdicionarAcao(Acao acao)
    {
        if (acao == null)
            throw new DomainException("Ação não pode ser nula.");

        if (!Ativo)
            throw new DomainException("Não é possível adicionar ação a risco inativo.");

        Acoes.Add(acao);
    }

    public void AlterarResponsavel(string novoResponsavel)
    {
        ValidarResponsavel(novoResponsavel);
        ResponsavelChave = novoResponsavel.Trim();
    }

    public void Desativar()
    {
        if (Acoes.Any(a => a.Ativa && !a.Concluida))
            throw new DomainException("Não é possível desativar risco com ações pendentes.");

        Ativo = false;
    }
}
```

### Exemplo 3: Value Object

```csharp
public class Cpf
{
    public string Numero { get; }

    public Cpf(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException("CPF é obrigatório.");

        var cpfLimpo = LimparCpf(numero);

        if (!CpfValido(cpfLimpo))
            throw new DomainException("CPF inválido.");

        Numero = cpfLimpo;
    }

    private string LimparCpf(string cpf)
    {
        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    private bool CpfValido(string cpf)
    {
        if (cpf.Length != 11) return false;
        if (cpf.Distinct().Count() == 1) return false; // 111.111.111-11

        // Validação dígitos verificadores
        var digito1 = CalcularDigito(cpf.Substring(0, 9));
        var digito2 = CalcularDigito(cpf.Substring(0, 10));

        return cpf.EndsWith($"{digito1}{digito2}");
    }

    private int CalcularDigito(string cpf)
    {
        var soma = 0;
        for (var i = 0; i < cpf.Length; i++)
            soma += int.Parse(cpf[i].ToString()) * (cpf.Length + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override bool Equals(object? obj)
    {
        return obj is Cpf other && Numero == other.Numero;
    }

    public override int GetHashCode() => Numero.GetHashCode();

    public override string ToString() => FormatarCpf(Numero);

    private string FormatarCpf(string cpf)
    {
        return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
    }
}

// Uso
public class Fornecedor : Entity
{
    public string Nome { get; private set; }
    public Cpf Cpf { get; private set; }

    public Fornecedor(string nome, Cpf cpf)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório.");

        Nome = nome.Trim();
        Cpf = cpf ?? throw new DomainException("CPF é obrigatório.");
    }
}
```

---

## Referências

### Livros e Artigos

- **[Domain-Driven Design: Tackling Complexity in the Heart of Software](https://www.domainlanguage.com/ddd/)** - Eric Evans (2003)
- **[Implementing Domain-Driven Design](https://vaughnvernon.com/iddd/)** - Vaughn Vernon (2013)
- **[Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)** - Robert C. Martin
- **[Effective Aggregate Design](https://www.dddcommunity.org/library/vernon_2011/)** - Vaughn Vernon

### Microsoft Docs

- **[Domain Model Layer](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/microservice-domain-model)** - Designing the domain model layer
- **[Domain Entities](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/net-core-microservice-domain-model)** - .NET Core microservice domain model
- **[Value Objects](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects)** - Implementing value objects

### Projeto +Digital A11732

- **[DT-004: Mapeamento Objeto-Relacional com EF Core](DT-004-mapeamento-objeto-relacional-ef-core.md)** - Configuração de mapeamento
- **[DT-005: Padrões de Estilo de Código Backend](DT-005-padroes-estilo-codigo-backend.md)** - Convenções C#
- **[DT-008: Estratégia de Testes Unitários](DT-008-padroes-testes-backend.md)** - Testes de domínio
- **[DT-016: Padrões da Camada de Aplicação](DT-016-padroes-camada-aplicacao.md)** - CQRS e handlers
- **[DT-018: Padrão de Implementação de Repositórios](DT-018-padrao-implementacao-repositorios.md)** - Interfaces de persistência
- **[backend-webapi-components.md](../arquitetura/backend-webapi-components.md)** - Arquitetura geral do backend
- **[entidade-dominio.instructions.md](../../.github/instructions/entidade-dominio.instructions.md)** - Instruction file para Copilot

---

## Notas de Implementação

### Refatoração de Entidades Legadas

Para entidades existentes que não seguem este padrão:

1. **Criar testes caracterização** antes de refatorar
2. **Adicionar construtor rico** mantendo construtor vazio para EF
3. **Converter setters públicos em privados** progressivamente
4. **Mover validações** para construtores e métodos de mutação
5. **Adicionar FK properties** onde faltam
6. **Criar testes unitários** para novas invariantes
7. **Executar migrations** se estrutura de tabela mudar

### Quando NÃO Aplicar

Este padrão se aplica à **camada de domínio**. Não aplicar em:

- **DTOs** da camada de Aplicação (Request/Response) - podem ter setters públicos
- **View Models** do frontend - não têm comportamento
- **Entidades de mapeamento** para sistemas externos - apenas estrutura de dados
- **Tabelas de configuração** simples - podem usar modelo anêmico

### Ferramentas Auxiliares

- **xUnit**: framework de testes para testes unitários de domínio
- **FluentAssertions**: assertions mais legíveis em testes
- **AutoFixture**: geração de dados para testes
- **Bogus**: geração de dados fake realistas

---

**Data de Criação**: 2026-03-22  
**Última Atualização**: 2026-03-22  
**Status**: ✅ Aprovado  
**Autor**: Equipe Backend +Digital A11732
