# DT-003: Padrões de Nomenclatura de Objetos de Banco de Dados SQL Server
 
> **Metadados do Documento**
> **Componente:** `Banco de Dados`
> **Tipo:** Decisão Técnica
>
> **Propósito:** Garantir consistência e manutenibilidade nas nomenclaturas de objetos de BD do projeto **Sistema de Gestão de Contratos — ALE Combustíveis** (PIPREVENDA-1680)
>
> **Quando usar:** Ao criar ou modificar objetos de banco de dados SQL Server (tabelas, colunas, índices, constraints, schemas, views, functions, procedures, triggers)
>
> **Palavras-chave:** `sql-server` `t-sql` `nomenclatura` `padrões` `convenções` `banco-de-dados` `ale`
 
## Contexto
 
O projeto **Sistema de Gestão de Contratos (ALE Combustíveis)** utiliza **SQL Server** como banco de dados principal. Em ambiente de desenvolvimento local, a instância é acessada em `localhost`, database **`mk_gestaoContrato`**.
 
O backend é construído em **.NET 10** com Clean Architecture (solução de referência `PortalAle`), persistência via **EF Core** na camada `Data.SqlServer`, com um contexto de staging isolado (`Data.SapStaging`) para a ingestão vinda do SAP via ADF. As entidades de domínio (`Contrato`, `Aditivo`, `Guarda-chuva`, `Grupo Econômico`, `Reappraise`) são modeladas em C#, em PascalCase.
 
Este documento estabelece um padrão de nomenclatura **próprio deste projeto**, definido para SQL Server e para este cliente (ALE). Ele não deriva de nem se baseia em normas de outro cliente ou projeto — é uma convenção nova, pensada para as características do SQL Server e para a integração natural com .NET/EF Core.
 
### Problema
 
- **Inconsistências** de nomenclatura entre módulos (E1–E6) e integrações (SAP, PCR, Elaw, Data Lake)
- **Dificuldade de manutenção** sem um padrão único, especialmente com múltiplos desenvolvedores e integrações externas (SAP, PCR, Elaw) que trazem seus próprios formatos de campo, exigindo um "De-Para" claro
- **Necessidade de alinhamento com o ecossistema .NET/EF Core**, já que o domínio é modelado em C# (PascalCase) e persistido via EF Core
- **Rastreabilidade** de objetos e de seus propósitos através de nomenclatura padronizada, dado o volume do projeto (+20 mil contratos)
### Necessidades de Negócio
 
1. Garantir **qualidade e governança** de dados desde o início do projeto (nenhuma linha de código de produto foi escrita ainda — este é o momento certo para fixar o padrão)
2. Facilitar **onboarding** de novos desenvolvedores no time
3. Manter **compatibilidade** e previsibilidade para as integrações (SAP, PCR, Elaw, Data Lake) e para ferramentas de auditoria/observabilidade já previstas na arquitetura
4. Assegurar **rastreabilidade** e **documentação automatizada** via extended properties
---
 
## Decisão
 
Adotaremos um padrão de nomenclatura de objetos SQL Server desenhado especificamente para o projeto ALE, seguindo princípios de normalização, boas práticas de modelagem e as convenções idiomáticas do SQL Server/.NET.
 
### Princípios Fundamentais
 
1. **PascalCase** para todos os objetos de banco de dados (tabelas, colunas, views, procedures, functions, triggers)
2. **Palavras reservadas T-SQL em MAIÚSCULAS** (SELECT, INSERT, CREATE, etc.) — apenas as palavras reservadas da linguagem, não os identificadores
3. **Singular** para nomes de tabelas e colunas
4. **Nomenclatura descritiva** e auto-explicativa
5. **Código de classe** (2 letras) como prefixo do nome da coluna, indicando o propósito/tipo do dado — **sem** o mnemônico da tabela (decisão específica deste projeto, ver seção 4 abaixo)
6. **Extended properties obrigatórias** (`sp_addextendedproperty`) em todos os objetos, como forma de documentação automatizada
7. **Sem underscore** para separar termos dentro de um mesmo identificador — a separação é feita pela própria capitalização (PascalCase)
### Por que PascalCase no SQL Server?
 
O SQL Server se comporta de forma diferente do PostgreSQL nesse aspecto:
 
- A instância roda, por padrão, com uma **collation case-insensitive** (ex.: `SQL_Latin1_General_CP1_CI_AS`), então `Contrato`, `contrato` e `CONTRATO` são tratados como o **mesmo objeto** em comparações — mas o SQL Server **preserva a caixa exatamente como foi digitada na criação** para fins de exibição (em `sys.tables`, no SSMS, etc.)
- Diferente do PostgreSQL, **não é necessário usar colchetes** (`[Contrato]`, equivalente às aspas duplas do Postgres) para preservar a caixa — o comportamento padrão já preserva
- PascalCase é a convenção idiomática de T-SQL/SSMS (scripts auto-gerados, IntelliSense) e, mais importante, é a convenção padrão do **Entity Framework Core**, que mapeia classes e propriedades C# (PascalCase) diretamente para tabelas e colunas
- Como usamos códigos de classe nas colunas (ver seção 4), o mapeamento não será 1:1 automático com as propriedades das entidades de domínio — será necessário configurar explicitamente via Fluent API (`HasColumnName`) ou `IEntityTypeConfiguration<T>`. Isso é uma escolha consciente (ver "Consequências")
---
 
## Padrões de Nomenclatura
 
### 1. Regras Gerais
 
Todos os nomes devem:
 
- Começar com uma **letra**
- Conter apenas **letras (A-Z, a-z)** e **números (0-9)** — sem underscore, acentos, caracteres especiais ou espaços
- Respeitar o limite de **128 caracteres** do SQL Server (recomendado máximo de 30-40 para legibilidade)
- **Evitar palavras reservadas** do T-SQL como nome de objeto (`User`, `Order`, `Group`, `Identity`, `Key`, `Table`) — em especial, **nunca usar `Timestamp`** como nome de coluna: no SQL Server esse é um tipo especial (sinônimo de `rowversion`), não uma data/hora
```sql
-- ✅ Válidos
CREATE TABLE Contrato (...);
CREATE INDEX IX_Contrato_NrCnpj ON Contrato (NrCnpj);
 
-- ❌ Inválidos
CREATE TABLE 1Contrato (...);        -- Começa com número
CREATE TABLE Contrato_Item (...);    -- Underscore não é usado neste padrão
CREATE TABLE [Contrato] (...);       -- Colchetes desnecessários (nome já é válido sem eles)
CREATE TABLE [Group] (...);          -- Nome reservado (evitar mesmo entre colchetes)
```
 
---
 
### 2. Nomenclatura de Schemas
 
**Padrão:** `dbo` (schema padrão, recomendado para o domínio principal) ou um schema dedicado por contexto.
 
Dado que a arquitetura já separa `Data.SqlServer` (domínio) de `Data.SapStaging` (staging da ingestão SAP), o mesmo isolamento é refletido em schemas:
 
```sql
-- Schema padrão do domínio (recomendado, sem necessidade de criação explícita)
-- dbo.Contrato, dbo.Aditivo, dbo.GrupoEconomico
 
-- Schemas dedicados por contexto, quando necessário
CREATE SCHEMA Stg AUTHORIZATION dbo;  -- staging da integração SAP (ADF)
CREATE SCHEMA Aud AUTHORIZATION dbo;  -- tabelas/objetos de auditoria
```
 
---
 
### 3. Nomenclatura de Tabelas
 
**Padrão:** `<NomeEntidade>` (singular, PascalCase, sem prefixos)
 
**Regras:**
 
- Nome no **singular**
- **PascalCase**, sem colchetes
- Máximo **30 caracteres** (recomendado)
- Sem underscore — capitalização separa os termos
- Evitar preposições e artigos
```sql
-- ✅ Exemplos corretos
CREATE TABLE Contrato (...);
CREATE TABLE Aditivo (...);
CREATE TABLE GrupoEconomico (...);
CREATE TABLE Reappraise (...);
CREATE TABLE ContratoItem (...);
 
-- ❌ Exemplos incorretos
CREATE TABLE Contratos (...);        -- Plural
CREATE TABLE ContratoDoRevendedor (...); -- Preposição "Do"
CREATE TABLE tbl_Contrato (...);     -- Prefixo desnecessário
CREATE TABLE contrato (...);         -- Não segue PascalCase
```
 
**Documentação obrigatória (extended property):**
 
```sql
EXEC sp_addextendedproperty
    @name = N'MS_Description',
    @value = N'Cadastro consolidado de contratos (Rede/GRR/B2B)',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'Contrato';
```
 
---
 
### 4. Nomenclatura de Colunas
 
**Padrão:** `<CódigoClasse><Descrição>` — **sem** o mnemônico da tabela como prefixo.
 
> **Decisão específica deste projeto:** diferente de padrões que prefixam a coluna com uma abreviação da tabela (ex.: `FuncCdFuncionario`), aqui a coluna carrega apenas o **código de classe** + a **descrição**. O nome da tabela já fica explícito no contexto de qualquer JOIN/consulta (via alias) e nas entidades EF Core, tornando o prefixo redundante.
 
**Regras:**
 
- **PascalCase**, sem underscore
- Máximo **30 caracteres** (recomendado)
- Evitar preposições e artigos
- Evitar redundância entre o código de classe e a descrição
#### 4.1 Códigos de Classe
 
| Código | Descrição            | Tipo SQL Server recomendado          | Exemplo                              |
| ------ | -------------------- | ------------------------------------- | ------------------------------------- |
| **Id** | Identificador (PK/FK, auto-incremento) | `INT IDENTITY(1,1)` / `BIGINT IDENTITY(1,1)` | `Id`, `IdGrupoEconomico`      |
| **Cd** | Código (não auto-incremento, ex.: código externo/de negócio) | `INT`, `VARCHAR(n)`     | `CdSap`, `CdSegmento`                |
| **Ds** | Descrição             | `NVARCHAR(n)`                         | `DsProduto`                           |
| **Vl** | Valor monetário       | `DECIMAL(p,s)`                        | `VlTotal DECIMAL(15,2)`               |
| **In** | Indicador (booleano)  | `BIT`                                 | `InAtivo BIT`                         |
| **Dt** | Data                  | `DATE`                                | `DtAdmissao DATE`                     |
| **Nm** | Nome                  | `NVARCHAR(n)`                         | `NmCompleto NVARCHAR(100)`            |
| **Nr** | Número (identificador não sequencial) | `INT`, `VARCHAR(n)`  | `NrCnpj VARCHAR(14)`, `NrContrato`    |
| **Md** | Medida                | `DECIMAL(p,s)`                        | `MdVolume DECIMAL(10,3)`              |
| **Qn** | Quantidade             | `INT`, `DECIMAL(p,s)`                 | `QnDependentes INT`                   |
| **Sg** | Sigla                  | `CHAR(n)`, `VARCHAR(n)`               | `SgUf CHAR(2)`                        |
| **Pr** | Percentual             | `DECIMAL(5,2)`                        | `PrComissao DECIMAL(5,2)`             |
| **Tx** | Texto livre            | `NVARCHAR(MAX)`                       | `TxObservacao NVARCHAR(MAX)`          |
| **Mm** | Multimídia / binário   | `VARBINARY(MAX)`                      | `MmAnexo VARBINARY(MAX)`              |
| **Dh** | Data com hora e fuso   | `DATETIMEOFFSET`                      | `DhCriacao DATETIMEOFFSET`            |
| **Js** | JSON                   | `NVARCHAR(MAX)` (com `ISJSON` check)  | `JsMetadados NVARCHAR(MAX)`           |
 
> **Atenção (armadilha específica do SQL Server):** o tipo `TIMESTAMP` **não é** uma data/hora no T-SQL — é um sinônimo legado de `ROWVERSION` (contador binário interno de concorrência). Para data/hora com fuso, use sempre `DATETIME2` ou `DATETIMEOFFSET`, nunca `TIMESTAMP`.
 
**Evitar redundância:**
 
```sql
-- ❌ Redundante
IdIdContrato
DtDataNascimento
NmNomePai
 
-- ✅ Correto
IdContrato
DtNascimento
NmPai
```
 
**Documentação obrigatória:**
 
```sql
EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = N'Código único do contrato',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'Contrato',
    @level2type = N'COLUMN', @level2name = 'IdContrato';
```
 
---
 
### 5. Constraints
 
Aqui seguimos a convenção idiomática do SQL Server/SSMS (prefixos em maiúsculas), que já referencia o nome da tabela — a omissão de prefixo é uma decisão restrita às **colunas** (seção 4), não às constraints.
 
#### 5.1 Chave Primária
 
**Padrão:** `PK_<Tabela>`
 
```sql
ALTER TABLE Contrato
ADD CONSTRAINT PK_Contrato PRIMARY KEY (IdContrato);
```
 
#### 5.2 Chave Estrangeira
 
**Padrão:** `FK_<TabelaFilha>_<TabelaPai>`
 
```sql
ALTER TABLE Contrato
ADD CONSTRAINT FK_Contrato_GrupoEconomico
FOREIGN KEY (IdGrupoEconomico)
REFERENCES GrupoEconomico (IdGrupoEconomico);
```
 
#### 5.3 Unique
 
**Padrão:** `UQ_<Tabela>_<Descrição>`
 
```sql
ALTER TABLE Contrato ADD CONSTRAINT UQ_Contrato_NrCnpjPcr UNIQUE (NrCnpj, NrPcr);
```
 
#### 5.4 Check
 
**Padrão:** `CK_<Tabela>_<Descrição>`
 
```sql
-- Templates comuns
CHECK (InAtivo IN (0, 1))                        -- BIT nativo, não precisa de CHECK, mas exemplos análogos:
CHECK (VlValor > 0)                              -- Maior que zero
CHECK (PrPercentual BETWEEN 0 AND 100)           -- Percentuais
CHECK (DtFim >= DtInicio)                        -- Data fim maior que início
 
-- Exemplos práticos
ALTER TABLE Contrato ADD CONSTRAINT CK_Contrato_VlContratado
CHECK (VlContratado > 0);
 
ALTER TABLE Contrato ADD CONSTRAINT CK_Contrato_DtVigencia
CHECK (DtFimVigencia >= DtInicioVigencia);
```
 
#### 5.5 Default
 
**Padrão:** `DF_<Tabela>_<Coluna>`
 
```sql
ALTER TABLE Contrato
ADD CONSTRAINT DF_Contrato_InAtivo DEFAULT (1) FOR InAtivo;
 
ALTER TABLE Contrato
ADD CONSTRAINT DF_Contrato_DhInclusao DEFAULT (SYSDATETIMEOFFSET()) FOR DhInclusao;
```
 
---
 
### 6. Índices
 
#### 6.1 Índice de FK
 
**Padrão:** `IX_<TabelaFilha>_<Coluna>`
 
```sql
CREATE INDEX IX_Contrato_IdGrupoEconomico
ON Contrato (IdGrupoEconomico);
```
 
#### 6.2 Índice Secundário / Único
 
**Padrão:** `IX_<Tabela>_<Descrição>` (o `UNIQUE` é indicado pela palavra-chave, não por prefixo diferente)
 
```sql
CREATE INDEX IX_Contrato_NrCnpj ON Contrato (NrCnpj);
CREATE UNIQUE INDEX IX_Contrato_NrPcr ON Contrato (NrPcr);
```
 
---
 
### 7. Views
 
**Padrão:** `Vw<NomeEntidade>`
 
```sql
CREATE OR ALTER VIEW VwContratoAtivo AS
SELECT
    IdContrato,
    NrCnpj,
    NrPcr,
    DtInicioVigencia,
    DtFimVigencia,
    VlContratado
FROM Contrato
WHERE InAtivo = 1;
GO
 
EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = N'Visão de contratos ativos',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'VIEW',   @level1name = 'VwContratoAtivo';
```
 
---
 
### 8. Sequences
 
**Padrão:** `Seq<Descrição>`
 
> No SQL Server, para colunas de identificador de tabela, prefira **sempre** `IDENTITY(1,1)` em vez de `SEQUENCE` manual — é mais simples e é o padrão que o EF Core espera por convenção. Use `SEQUENCE` apenas quando o número precisa ser compartilhado entre múltiplas tabelas ou gerado antes do INSERT.
 
```sql
CREATE SEQUENCE SeqNumeroContrato
START WITH 1
INCREMENT BY 1
NO CACHE;
```
 
---
 
### 9. Functions
 
**Padrão:** `Fn<Descrição>`
 
```sql
CREATE OR ALTER FUNCTION FnCalculaIdade (@DtNascimento DATE)
RETURNS INT
AS
BEGIN
    RETURN DATEDIFF(YEAR, @DtNascimento, GETDATE())
        - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, @DtNascimento, GETDATE()), @DtNascimento) > GETDATE()
               THEN 1 ELSE 0 END;
END;
GO
```
 
---
 
### 10. Stored Procedures
 
**Padrão:** `Usp<Descrição><Operação>`
 
**Operações:** `Ins`, `Upd`, `Del`, `Sel` (omitir para genérico)
 
> **Importante:** não use o prefixo `sp_` (comum em outros bancos/em código legado). No SQL Server, procedures com nome iniciado por `sp_` fazem o engine **sempre checar primeiro o banco `master`** antes do banco atual, gerando overhead de busca e risco de colisão de nome com procedures de sistema. Por isso o prefixo aqui é `Usp` (*User Stored Procedure*).
 
```sql
CREATE OR ALTER PROCEDURE UspAtualizaValorContratoUpd
    @IdContrato INT,
    @VlNovoValor DECIMAL(15,2)
AS
BEGIN
    SET NOCOUNT ON;
 
    UPDATE Contrato
    SET VlContratado = @VlNovoValor,
        DhAlteracao = SYSDATETIMEOFFSET()
    WHERE IdContrato = @IdContrato;
END;
GO
```
 
---
 
### 11. Triggers
 
**Padrão:** `Tr<Momento><Tabela><Operação>`
 
**Momentos:** `Af` (AFTER), `Io` (INSTEAD OF) — o SQL Server não possui `BEFORE`, apenas `AFTER`/`INSTEAD OF`
**Operações:** `Ins`, `Upd`, `Del`, `Iu` (INSERT/UPDATE combinados)
 
> No T-SQL não existem os pseudo-registros `NEW`/`OLD` nem a variável `TG_OP` do PL/pgSQL. O equivalente são as tabelas virtuais **`INSERTED`** e **`DELETED`**, e a operação é inferida pela combinação de linhas presentes nelas.
 
```sql
CREATE OR ALTER TRIGGER TrAfContratoIu
ON Contrato
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
 
    -- INSERT: só existem linhas em INSERTED
    UPDATE c
    SET DhInclusao = SYSDATETIMEOFFSET()
    FROM Contrato c
    INNER JOIN INSERTED i ON i.IdContrato = c.IdContrato
    WHERE NOT EXISTS (SELECT 1 FROM DELETED d WHERE d.IdContrato = i.IdContrato);
 
    -- UPDATE: linhas existem em INSERTED e em DELETED
    UPDATE c
    SET DhAlteracao = SYSDATETIMEOFFSET()
    FROM Contrato c
    INNER JOIN INSERTED i ON i.IdContrato = c.IdContrato
    INNER JOIN DELETED d ON d.IdContrato = i.IdContrato;
END;
GO
```
 
---
 
### 12. Views Indexadas (equivalente a Materialized Views)
 
> O SQL Server **não tem `MATERIALIZED VIEW`**. O equivalente funcional é uma **view indexada**: uma view criada com `SCHEMABINDING`, com um índice clusterizado único criado sobre ela. Diferente de uma materialized view do Postgres, o conteúdo é mantido **automaticamente sincronizado** pelo engine a cada alteração nas tabelas base (não existe `REFRESH MATERIALIZED VIEW`).
 
**Padrão:** `Vw<Entidade>` (mesmo padrão de views comuns)
 
```sql
CREATE OR ALTER VIEW VwContratoAtivoIdx
WITH SCHEMABINDING
AS
SELECT
    IdContrato,
    NrCnpj,
    DtInicioVigencia
FROM dbo.Contrato
WHERE InAtivo = 1;
GO
 
CREATE UNIQUE CLUSTERED INDEX IX_VwContratoAtivoIdx_IdContrato
ON VwContratoAtivoIdx (IdContrato);
GO
```
 
---
 
## Template Completo de Tabela
 
> Exemplo ilustrativo do padrão aplicado — os campos abaixo servem apenas para demonstrar a convenção de nomenclatura, não representam o modelo de dados definitivo do contrato (que depende do "De-Para" e das integrações SAP/PCR/Elaw ainda em definição).
 
```sql
-- ========================================
-- Tabela: Contrato
-- Descrição: Base contratual consolidada
-- ========================================
 
CREATE TABLE Contrato (
    -- Chave primária (auto-incremento)
    IdContrato INT IDENTITY(1,1) NOT NULL,
 
    -- Identificadores de negócio
    NrCnpj VARCHAR(14) NOT NULL,
    NrPcr VARCHAR(20) NOT NULL,
    CdSap VARCHAR(20) NULL,
 
    -- Relacionamentos
    IdGrupoEconomico INT NULL,
 
    -- Classificação
    DsSegmento NVARCHAR(20) NOT NULL,
 
    -- Vigência
    DtInicioVigencia DATE NOT NULL,
    DtFimVigencia DATE NOT NULL,
 
    -- Indicadores
    InAtivo BIT NOT NULL,
 
    -- Valores
    VlContratado DECIMAL(15,2) NULL,
 
    -- Medidas / quantidades
    MdVolumeContratado DECIMAL(10,3) NULL,
 
    -- Texto livre
    TxObservacao NVARCHAR(MAX) NULL,
 
    -- Controle (auditoria)
    DhInclusao DATETIMEOFFSET NOT NULL,
    DhAlteracao DATETIMEOFFSET NULL,
 
    CONSTRAINT PK_Contrato PRIMARY KEY (IdContrato),
    CONSTRAINT UQ_Contrato_NrPcr UNIQUE (NrPcr),
    CONSTRAINT CK_Contrato_DtVigencia CHECK (DtFimVigencia >= DtInicioVigencia),
    CONSTRAINT CK_Contrato_VlContratado CHECK (VlContratado > 0),
    CONSTRAINT DF_Contrato_InAtivo DEFAULT (1) FOR InAtivo,
    CONSTRAINT DF_Contrato_DhInclusao DEFAULT (SYSDATETIMEOFFSET()) FOR DhInclusao,
    CONSTRAINT FK_Contrato_GrupoEconomico
        FOREIGN KEY (IdGrupoEconomico)
        REFERENCES GrupoEconomico (IdGrupoEconomico)
);
GO
 
-- Documentação obrigatória
EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = N'Base contratual consolidada (Rede/GRR/B2B)',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'Contrato';
 
EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = N'Código único do contrato',
    @level0type = N'SCHEMA', @level0name = 'dbo',
    @level1type = N'TABLE',  @level1name = 'Contrato',
    @level2type = N'COLUMN', @level2name = 'IdContrato';
 
-- Índices
CREATE INDEX IX_Contrato_IdGrupoEconomico ON Contrato (IdGrupoEconomico);
CREATE INDEX IX_Contrato_NrCnpj ON Contrato (NrCnpj);
GO
 
-- View de contratos ativos
CREATE OR ALTER VIEW VwContratoAtivo AS
SELECT
    IdContrato,
    NrCnpj,
    NrPcr,
    DtInicioVigencia,
    DtFimVigencia,
    VlContratado
FROM Contrato
WHERE InAtivo = 1;
GO
 
-- Trigger de auditoria
CREATE OR ALTER TRIGGER TrAfContratoIu
ON Contrato
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
 
    UPDATE c
    SET DhAlteracao = SYSDATETIMEOFFSET()
    FROM Contrato c
    INNER JOIN INSERTED i ON i.IdContrato = c.IdContrato
    INNER JOIN DELETED d ON d.IdContrato = i.IdContrato;
END;
GO
```
 
---
 
## Tipos de Dados Recomendados (SQL Server)
 
### Identificadores com Auto-incremento
 
**✅ Recomendação: `IDENTITY(1,1)`**
 
```sql
-- Recomendado
IdContrato INT IDENTITY(1,1) NOT NULL
IdGrupoEconomico BIGINT IDENTITY(1,1) NOT NULL
```
 
**Justificativas:**
 
1. É o mecanismo **nativo e idiomático** do SQL Server para auto-incremento (equivalente ao `SERIAL`/`IDENTITY` do Postgres, mas sem a sintaxe `GENERATED ... AS IDENTITY` do padrão SQL:2003 — o SQL Server usa sua própria sintaxe desde sempre)
2. É o que o **EF Core espera por convenção** para chaves primárias `int`/`long` — dispensa configuração adicional de geração de valor
3. Permite customização via `IDENTITY(seed, increment)`
### Tipos Recomendados por Categoria
 
```sql
-- Identificadores / códigos
INT               -- Identificadores/códigos médios — RECOMENDADO como padrão
BIGINT            -- Identificadores muito grandes (ex.: alto volume histórico)
SMALLINT          -- Códigos pequenos, enumerações internas
 
-- Valores monetários
DECIMAL(15,2)     -- Padrão para valores monetários e financeiros (evitar FLOAT/REAL)
DECIMAL(10,2)
 
-- Percentuais e taxas
DECIMAL(5,2)      -- Até 999,99%
DECIMAL(3,2)      -- Até 9,99%
 
-- Quantidades e medidas
INT               -- Quantidades inteiras
DECIMAL(10,3)     -- Medidas com casas decimais
 
-- Textos
CHAR(1)           -- Indicadores textuais quando BIT não se aplica (ex.: 'S'/'N' vindo de integração)
CHAR(2)           -- UF, siglas fixas
VARCHAR(14)       -- CNPJ (ASCII, sem necessidade de Unicode)
NVARCHAR(100)     -- Nomes (Unicode — acentos, caracteres especiais)
NVARCHAR(200)     -- Descrições
NVARCHAR(MAX)     -- Textos longos
 
-- Datas
DATE                    -- Data simples, sem hora
DATETIME2               -- Data com hora (evitar o legado DATETIME, menos preciso)
DATETIMEOFFSET          -- Data com hora e fuso horário (recomendado para auditoria/integrações)
 
-- ⚠️ Nunca usar TIMESTAMP como tipo de data — no SQL Server é sinônimo de ROWVERSION
 
-- Binários
VARBINARY(MAX)    -- Arquivos, anexos, imagens
 
-- Booleanos
BIT               -- Tipo nativo para indicadores verdadeiro/falso
 
-- JSON
NVARCHAR(MAX)     -- Armazenamento de JSON (usar funções nativas ISJSON/JSON_VALUE/JSON_QUERY)
```
 
---
 
## Validações Comuns
 
```sql
-- Indicadores (BIT dispensa CHECK; CHAR(1) vindo de integração externa pode precisar)
CHECK (CdStatusIntegracao IN ('A', 'I', 'P', 'C'))  -- Ativo, Inativo, Pendente, Cancelado
 
-- Valores monetários e numéricos
CHECK (VlValor > 0)
CHECK (VlValor >= 0)
CHECK (PrPercentual BETWEEN 0 AND 100)
 
-- Datas
CHECK (DtFim >= DtInicio)
CHECK (DtNascimento < CAST(GETDATE() AS DATE))
```
 
---
 
## Alternativas Consideradas
 
### Alternativa 1: snake_case minúsculo
 
**Motivo da rejeição:** embora funcione tecnicamente no SQL Server (a collation padrão é case-insensitive), não é a convenção idiomática do ecossistema T-SQL/.NET e criaria divergência desnecessária com o padrão de nomenclatura das entidades de domínio em C# (PascalCase), aumentando o atrito de mapeamento no EF Core.
 
### Alternativa 2: Nomenclatura sem código de classe
 
**Motivo da rejeição:** o código de classe (`Vl`, `Dt`, `In`, etc.) facilita a identificação rápida do propósito/tipo de um campo em queries complexas e em relatórios (ex.: Book Executivo, E6), especialmente relevante dado o volume de +20 mil contratos e múltiplas fontes de integração.
 
### Alternativa 3: Manter o mnemônico da tabela como prefixo da coluna
 
**Motivo da rejeição:** decisão explícita deste projeto — o nome da tabela já é explícito no contexto de qualquer JOIN/alias e nas entidades EF Core; manter o mnemônico tornaria os nomes de coluna mais longos sem ganho real de legibilidade.
 
---
 
## Consequências
 
### Positivas ✅
 
- ✅ Convenção **coerente com o ecossistema .NET/EF Core** usado no projeto
- ✅ **Rastreabilidade** via nomenclatura padronizada e extended properties
- ✅ Facilita **onboarding** de novos desenvolvedores
- ✅ Nomes de coluna mais **curtos e legíveis** (sem mnemônico de tabela redundante)
- ✅ Reduz risco de armadilhas específicas do SQL Server (ex.: uso indevido de `TIMESTAMP`, prefixo `sp_`)
### Negativas ⚠️
 
- ⚠️ Como as colunas usam código de classe (`IdContrato`, `VlContratado`), o mapeamento EF Core **não é 1:1 automático** com propriedades de domínio que não sigam o mesmo padrão — exige configuração explícita via Fluent API (`HasColumnName`) em `IEntityTypeConfiguration<T>`
- ⚠️ Curva de aprendizado inicial para quem não está familiarizado com os códigos de classe
- ⚠️ Necessidade de disciplina para manter consistência entre times/PRs
### Neutras ℹ️
 
- ℹ️ Requer configuração de ferramentas de qualidade (linters compatíveis com dialeto T-SQL)
- ℹ️ Necessita alinhamento com o time de arquitetura (Yuri Najar) sobre a configuração de mapeamento EF Core (`IEntityTypeConfiguration<T>` por entidade)
- ℹ️ Demanda revisão de código para garantir aderência, especialmente nas migrations geradas pelo EF Core
---
 
## Conformidade e Validação
 
### Ferramentas de Validação
 
- **SQL Server Data Tools (SSDT)**: projeto de banco versionado, comparação de schema (DACPAC)
- **SQLFluff** (dialeto `tsql`): linting de SQL
- **Migrations do EF Core**: revisão obrigatória do script gerado antes de aplicar, para garantir aderência ao padrão (o EF Core, por convenção própria, tende a gerar nomes diferentes dos aqui definidos se não houver configuração explícita)
- **Scripts customizados**: validação de nomenclatura via `sys.tables`/`sys.columns`
### Processo de Revisão
 
1. Todo script SQL (ou migration EF Core) deve ser revisado quanto à aderência aos padrões
2. Pipeline Azure DevOps deve incluir validação automatizada de nomenclatura antes do deploy
3. Code review deve verificar:
   - ✅ Nomenclatura de objetos (PascalCase, sem underscore)
   - ✅ Presença de extended properties (`MS_Description`)
   - ✅ Uso correto de tipos de dados (nunca `TIMESTAMP` como data, `DECIMAL` para valores monetários)
   - ✅ Implementação de constraints e configuração explícita de mapeamento EF Core quando necessário
---
 
## Referências
 
- **Projeto:** Sistema de Gestão de Contratos — ALE Combustíveis (PIPREVENDA-1680)
- **Contexto de projeto:** `CONTEXTO-COMPLETO-PROJETO.md`
- **Microsoft Docs — Identifiers:** https://learn.microsoft.com/en-us/sql/relational-databases/databases/database-identifiers
- **Microsoft Docs — Identity Columns:** https://learn.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql-identity-property
- **Microsoft Docs — Indexed Views:** https://learn.microsoft.com/en-us/sql/relational-databases/views/create-indexed-views
---
 
## Histórico de Revisões
 
| Data       | Versão | Autor         | Descrição                                                              |
| ---------- | ------ | ------------- | ------------------------------------------------------------------------ |
| 2026-01-23 | 1.0    | Líder Técnico | Versão inicial (PostgreSQL, base em norma de outro cliente)             |
| 2026-07-28 | 2.0    | Líder Técnico | Reescrito para SQL Server, projeto ALE (`mk_gestaoContrato`, localhost); nova convenção própria, sem mnemônico de tabela nas colunas |
 
---
 
## Aprovações
 
| Papel                | Nome       | Data       | Assinatura |
| --------------------- | ---------- | ---------- | ---------- |
| Líder Técnico         | [Pendente] | [Pendente] |            |
| Arquiteto de Solução  | [Pendente] | [Pendente] |            |
| Product Manager       | [Pendente] | [Pendente] |            |
 
---
 
## Anexos
 
### Anexo A: Checklist de Validação de Scripts SQL
 
```markdown
- [ ] Nomes de tabelas em PascalCase, singular
- [ ] Nomes de colunas seguem padrão CódigoClasse + Descrição (PascalCase, sem mnemônico de tabela)
- [ ] Códigos de classe corretos (Id, Cd, Ds, Vl, In, Dt, Nm, Nr, Md, Qn, Sg, Pr, Tx, Mm, Dh, Js)
- [ ] Nenhuma coluna de data/hora nomeada ou tipada como TIMESTAMP
- [ ] Constraints nomeadas conforme padrão (PK_, FK_, UQ_, CK_, DF_)
- [ ] Índices nomeados conforme padrão (IX_)
- [ ] Nenhuma procedure com prefixo sp_
- [ ] Extended properties (MS_Description) presentes em tabelas e colunas
- [ ] Tipos de dados adequados (DECIMAL para valores monetários, NVARCHAR para texto com acentuação, BIT para indicadores)
- [ ] Validações (CHECK constraints) implementadas quando necessário
- [ ] Defaults configurados onde aplicável
- [ ] Views/Functions/Procedures/Triggers seguem nomenclatura padrão (Vw, Fn, Usp, Tr)
- [ ] Configuração explícita de mapeamento no EF Core (HasColumnName) quando o nome de coluna difere da propriedade C#
```
 
### Anexo B: Scripts de Validação
 
```sql
-- Script para validar nomenclatura de tabelas
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    CASE
        WHEN t.name COLLATE Latin1_General_BIN LIKE '%[^a-zA-Z0-9]%' THEN 'Nome contém caracteres inválidos'
        WHEN t.name LIKE '%s' THEN 'Nome pode estar no plural (verificar)'
        WHEN LEN(t.name) > 30 THEN 'Nome muito longo (>30 caracteres)'
        ELSE 'OK'
    END AS Validacao
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'dbo';
 
-- Script para validar extended properties (documentação) obrigatórias
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    CASE
        WHEN ep.value IS NULL THEN 'Extended property (MS_Description) ausente na tabela'
        ELSE 'OK'
    END AS ValidacaoTabela
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.extended_properties ep
    ON ep.major_id = t.object_id
    AND ep.minor_id = 0
    AND ep.name = 'MS_Description'
WHERE s.name = 'dbo';
 
-- Script para detectar procedures com prefixo sp_ (anti-pattern no SQL Server)
SELECT name
FROM sys.procedures
WHERE name LIKE 'sp[_]%';
```
 
---
 
### Anexo C: Lista de Verificação de Corretude do Modelo de Dados