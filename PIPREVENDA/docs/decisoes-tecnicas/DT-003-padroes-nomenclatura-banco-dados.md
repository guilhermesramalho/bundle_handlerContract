# DT-003: Padrões de Nomenclatura de Objetos de Banco de Dados PostgreSQL

> **Metadados do Documento**  
> **Componente:** `Banco de Dados`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** Garantir consistência, manutenibilidade e aderência aos padrões corporativos Petrobras (PE-2TIC-00319) em nomenclaturas de objetos de BD
>
> **Quando usar:** Ao criar ou modificar objetos de banco de dados PostgreSQL (tabelas, colunas, índices, constraints, schemas, functions)
>
> **Palavras-chave:** `postgresql` `nomenclatura` `padrões` `pe-2tic-00319` `convenções` `banco-de-dados`

## Contexto

O projeto **+Digital (a11732)** utiliza PostgreSQL como banco de dados principal. Para garantir **consistência**, **manutenibilidade** e **aderência aos padrões corporativos Petrobras** (baseados na norma PE-2TIC-00319), é necessário estabelecer convenções claras de nomenclatura para todos os objetos de banco de dados.

### Problema

- **Inconsistências** em nomenclaturas entre diferentes módulos do sistema
- **Dificuldade de manutenção** devido à falta de padrões claros
- **Necessidade de aderência** aos padrões corporativos da Petrobras
- **Adaptação** dos padrões Oracle (PE-2TIC-00319) para **PostgreSQL**
- **Rastreabilidade** de objetos e seus propósitos através de nomenclatura padronizada

### Necessidades de Negócio

1. Garantir **qualidade e governança** de dados
2. Facilitar **onboarding** de novos desenvolvedores
3. Manter **compatibilidade** com ferramentas de análise e auditoria
4. Assegurar **rastreabilidade** e **documentação automatizada**

---

## Decisão

Adotaremos padrões de nomenclatura de objetos de banco de dados PostgreSQL **baseados na norma PE-2TIC-00319 da Petrobras**, adaptados às características específicas do PostgreSQL, seguindo princípios de **normalização** e **boas práticas de modelagem**.

### Princípios Fundamentais

1. **Nomes em minúsculas** para todos os objetos de banco de dados (tabelas, colunas, índices, etc.)
2. **Palavras reservadas SQL em MAIÚSCULAS** (SELECT, INSERT, CREATE, etc.)
3. **Singular e masculino** para nomes de tabelas e colunas
4. **Nomenclatura descritiva** e auto-explicativa
5. **Mnemônicos de até 4 caracteres** para colunas
6. **Códigos de classe padronizados** para colunas
7. **Comentários obrigatórios** em todos os objetos
8. **Uso de underscore (\_)** para separação de termos

### Importante: Por que minúsculas no PostgreSQL?

Apesar da norma PE-2TIC-00319 da Petrobras recomendar MAIÚSCULAS (baseada em Oracle), **o PostgreSQL tem comportamento diferente**:

- **Identificadores sem aspas** são automaticamente convertidos para **minúsculas**
- Mesmo que você escreva `CREATE TABLE FUNCIONARIO`, o PostgreSQL cria como `funcionario`
- Para forçar maiúsculas, seria necessário usar aspas duplas: `CREATE TABLE "FUNCIONARIO"` (não recomendado)
- Usar aspas duplas causa problemas: obriga usar aspas em **todas** as referências futuras

---

## Padrões de Nomenclatura

### 1. Regras Gerais

Todos os nomes devem:

- Começar com uma **letra**
- Conter apenas **letras (A-Z)**, **números (0-9)** e **underscore (\_)**
- Não usar acentos, caracteres especiais ou espaços
- Respeitar limite de **63 caracteres** do PostgreSQL (recomendado máximo de 30 para compatibilidade)

```sql
-- ✅ Válidos
CREATE TABLE funcionario (...);
CREATE INDEX in_func_cpf ON funcionario (func_nr_cpf);

-- ❌ Inválidos
CREATE TABLE 1funcionario (...);      -- Começa com número
CREATE TABLE funcionário (...);       -- Contém acento
CREATE TABLE "Funcionário" (...);     -- Case-sensitive com aspas (evitar)
CREATE TABLE FUNCIONARIO (...);       -- Será convertido para minúsculo pelo PostgreSQL
```

---

### 2. Nomenclatura de Schemas

**Padrão:** `<codigo_aplicacao>` ou `<area_negocio>`

```sql
-- Exemplos
CREATE SCHEMA a11732;           -- Schema principal da aplicação
CREATE SCHEMA a11732_audit;     -- Schema de auditoria
CREATE SCHEMA a11732_staging;   -- Schema de staging/temporário
```

---

### 3. Nomenclatura de Tabelas

**Padrão:** `<nome_entidade>` (singular, masculino, sem prefixos)

**Regras:**

- Nome no **singular** e **masculino**
- Letras **minúsculas** (sem aspas duplas)
- Máximo **30 caracteres** (recomendado)
- Termos separados por **underscore (\_) **
- **Mínimo 2 letras** por termo
- Evitar **preposições** e **artigos**

```sql
-- ✅ Exemplos corretos
CREATE TABLE funcionario (...);
CREATE TABLE saldo_fgts (...);
CREATE TABLE folha_ferias (...);
CREATE TABLE produto (...);
CREATE TABLE pedido_item (...);

-- ❌ Exemplos incorretos
CREATE TABLE funcionarios (...);     -- Plural
CREATE TABLE func_do_rh (...);       -- Preposição "do"
CREATE TABLE tbl_funcionario (...);  -- Prefixo desnecessário
CREATE TABLE "FUNCIONARIO" (...);    -- Aspas duplas (case-sensitive forçado)
```

**Comentários obrigatórios:**

```sql
COMMENT ON TABLE funcionario IS 'Cadastro de funcionários da empresa';
```

---

### 4. Nomenclatura de Colunas

**Padrão:** `<mnemônico>_<código_classe>_<descrição>`

**Regras:**

- Nome no **singular** e **masculino**
- Letras **minúsculas** (sem aspas duplas)
- Máximo **30 caracteres** (recomendado)
- Termos separados por **underscore (\_) **
- Evitar **preposições** e **artigos**
- Utilizar mnemônico da tabela de referência como prefixo da coluna
- Utilizar códigos de classe após mnemônico da tabela para definir propósito do campo

#### 4.1 Mnemônico de Tabela (4 caracteres)

| Palavras | Regra                       | Exemplo                          |
| -------- | --------------------------- | -------------------------------- |
| 1        | Primeiros 4 caracteres      | EMPREGADO → EMPR                 |
| 2        | 2 + 2 caracteres            | DM_EMPREGADO → DMEM              |
| 3        | 2 + 1 + 1 caracteres        | CT_CONTROLE_RELATORIO → CTCR     |
| 4+       | 2 + palavras significativas | MM_BASE_CONSULTA_ATRIBUTO → MMBA |

**Mnemônicos Comuns:**

```sql
func = funcionario
dept = departamento
prod = produto
pedo = pedido
item = item_pedido
clie = cliente
forn = fornecedor
esto = estoque
```

#### 4.2 Códigos de Classe

| Código | Descrição            | Tipo PostgreSQL                            | Exemplo                                                        |
| ------ | -------------------- | ------------------------------------------ | -------------------------------------------------------------- |
| **CD** | Código/Identificador | `INTEGER`, `BIGINT`                        | `func_cd_funcionario INTEGER`                                  |
| **DS** | Descrição            | `VARCHAR(n)`, `TEXT`                       | `prod_ds_produto VARCHAR(200)`                                 |
| **VL** | Valor monetário      | `NUMERIC(p,s)`, `MONEY`                    | `cont_vl_total NUMERIC(15,2)`                                  |
| **IN** | Indicador            | `CHAR(1)`, `BOOLEAN`                       | `func_in_ativo CHAR(1)` ou `BOOLEAN`                           |
| **DT** | Data                 | `DATE`                                     | `func_dt_admissao DATE`                                        |
| **NM** | Nome                 | `VARCHAR(n)`                               | `func_nm_completo VARCHAR(100)`                                |
| **NR** | Número               | `INTEGER`, `BIGINT`                        | `func_nr_matricula INTEGER`                                    |
| **MD** | Medida               | `NUMERIC(p,s)`                             | `equip_md_peso NUMERIC(10,3)`                                  |
| **QN** | Quantidade           | `INTEGER`, `NUMERIC`                       | `esto_qn_estoque INTEGER`                                      |
| **SG** | Sigla                | `CHAR(n)`, `VARCHAR(n)`                    | `esta_sg_uf CHAR(2)`                                           |
| **PR** | Percentual           | `NUMERIC(5,2)`                             | `func_pr_comissao NUMERIC(5,2)`                                |
| **CD** | Código (auto-incr)   | `INTEGER GENERATED BY DEFAULT AS IDENTITY` | `func_cd_funcionario INTEGER GENERATED BY DEFAULT AS IDENTITY` |
| **TX** | Texto livre          | `TEXT`, `VARCHAR(n)`                       | `equip_tx_observacao TEXT`                                     |
| **MM** | Multimídia           | `BYTEA`                                    | `func_mm_foto BYTEA`                                           |
| **DF** | Data com fuso        | `TIMESTAMP WITH TIME ZONE`                 | `prod_df_criacao TIMESTAMP WITH TIME ZONE`                     |
| **JS** | JSON                 | `JSON`, `JSONB`                            | `prod_js_metadados JSONB`                                      |

**Evitar Redundância:**

```sql
-- ❌ Redundante
FUNC_CD_CODIGO
FUNC_DT_DATA_NASCIMENTO
FUNC_NM_NOME_PAI

-- ✅ Correto
func_cd_funcionario
func_dt_nascimento
func_nm_pai
```

**Comentários obrigatórios:**

```sql
COMMENT ON COLUMN funcionario.func_cd_funcionario IS 'Código único do funcionário';
COMMENT ON COLUMN funcionario.func_nm_completo IS 'Nome completo do funcionário';
COMMENT ON COLUMN funcionario.func_in_ativo IS 'Indicador se funcionário está ativo (S/N ou TRUE/FALSE)';
```

---

### 5. Constraints

#### 5.1 Chave Primária

**Padrão:** `pk_<mnemônico>`

```sql
ALTER TABLE funcionario
ADD CONSTRAINT pk_func
PRIMARY KEY (func_cd_funcionario);
```

#### 5.2 Chave Estrangeira

**Padrão:** `fk_<mnem_pai>_<mnem_filho>_<descrição>`

```sql
ALTER TABLE funcionario
ADD CONSTRAINT fk_dept_func_lotacao
FOREIGN KEY (func_cd_departamento)
REFERENCES departamento (dept_cd_departamento);
```

#### 5.3 Unique

**Padrão:** `un_<mnemônico>_<descrição>`

```sql
ALTER TABLE funcionario ADD CONSTRAINT un_func_cpf UNIQUE (func_nr_cpf);
ALTER TABLE funcionario ADD CONSTRAINT un_func_matricula UNIQUE (func_nr_matricula);
```

#### 5.4 Check

**Padrão:** `ck_<mnemônico>_<descrição>`

```sql
-- Templates comuns
CHECK (campo_in_status IN ('A', 'I', 'P', 'C'))  -- Ativo, Inativo, Pendente, Cancelado
CHECK (campo_in_ativo IN ('S', 'N'))             -- Sim, Não
CHECK (campo_in_sexo IN ('M', 'F'))              -- Masculino, Feminino

-- Ou usando BOOLEAN (PostgreSQL nativo)
CHECK (campo_in_ativo IS TRUE OR campo_in_ativo IS FALSE)

-- Valores monetários e numéricos
CHECK (campo_vl_valor > 0)                       -- Maior que zero
CHECK (campo_vl_valor >= 0)                      -- Maior ou igual a zero
CHECK (campo_pr_percentual BETWEEN 0 AND 100)    -- Percentuais

-- Datas
CHECK (campo_dt_fim >= campo_dt_inicio)          -- Data fim maior que início
CHECK (campo_dt_nascimento < CURRENT_DATE)       -- Data no passado

-- Exemplos práticos
ALTER TABLE funcionario ADD CONSTRAINT ck_func_in_ativo
CHECK (func_in_ativo IN ('S', 'N'));

ALTER TABLE funcionario ADD CONSTRAINT ck_func_vl_salario
CHECK (func_vl_salario > 0);
```

#### 5.5 Default

**Padrão:** `df_<mnemônico>_<campo>`

```sql
ALTER TABLE funcionario
ALTER COLUMN func_in_ativo SET DEFAULT 'S';

ALTER TABLE funcionario
ALTER COLUMN func_dt_inclusao SET DEFAULT CURRENT_TIMESTAMP;
```

---

### 6. Índices

#### 6.1 Índice de FK

**Padrão:** `in_fk_<tabela_pai_mnem>_<tabela_filho_mnem>_<descrição>`

```sql
-- Template
CREATE INDEX in_fk_[parent_mnem]_[child_mnem]_[desc]
ON [child_table] ([fk_column]);

-- Exemplo
CREATE INDEX in_fk_dept_func_lotacao
ON funcionario (func_cd_departamento);
```

#### 6.2 Índice Secundário

**Padrão:** `in_<mnemônico>_<texto_significativo>`

```sql
-- Exemplos
CREATE INDEX in_func_cpf ON funcionario (func_nr_cpf);
CREATE INDEX in_func_email ON funcionario (func_nm_email);
CREATE INDEX in_func_nome ON funcionario (func_nm_completo);
```

#### 6.3 Índice Único

**Padrão:** `in_<mnemônico>_<texto_significativo>`

**Observação:** Índices únicos usam o mesmo prefixo `in_` que índices comuns, diferenciando-se apenas pela palavra-chave `UNIQUE`.

```sql
-- Exemplo
CREATE UNIQUE INDEX in_func_matricula
ON funcionario (func_nr_matricula);
```

---

### 7. Views

**Padrão:** `vw_<padrão_tabela>`

```sql
CREATE OR REPLACE VIEW vw_funcionario_ativo AS
SELECT
    func_cd_funcionario,
    func_nm_completo,
    func_dt_admissao,
    func_vl_salario
FROM funcionario
WHERE func_in_ativo = 'S';

COMMENT ON VIEW vw_funcionario_ativo IS 'Visão de funcionários ativos';
```

---

### 8. Sequences

**Padrão:** `seq_<mnemônico>_<campo>`

```sql
CREATE SEQUENCE seq_func_cd_funcionario
START WITH 1
INCREMENT BY 1
NO MAXVALUE
CACHE 1;

COMMENT ON SEQUENCE seq_func_cd_funcionario IS 'Sequência para geração de código de funcionário';
```

---

### 9. Functions

**Padrão:** `fn_<sistema>_<descrição>`

```sql
CREATE OR REPLACE FUNCTION fn_a11732_calcula_idade(
    p_dt_nascimento DATE
) RETURNS INTEGER AS $$
BEGIN
    RETURN EXTRACT(YEAR FROM AGE(CURRENT_DATE, p_dt_nascimento));
END;
$$ LANGUAGE plpgsql IMMUTABLE;

COMMENT ON FUNCTION fn_a11732_calcula_idade(DATE) IS 'Calcula idade a partir da data de nascimento';
```

---

### 10. Stored Procedures

**Padrão:** `sp_<sistema>_<descrição>_<operação>`

**Operações:** `ins`, `upd`, `del`, `sel` (omitir para genérico)

```sql
CREATE OR REPLACE PROCEDURE sp_a11732_atualiza_salario_upd(
    p_cd_funcionario INTEGER,
    p_vl_novo_salario NUMERIC
) LANGUAGE plpgsql AS $$
BEGIN
    UPDATE funcionario
    SET func_vl_salario = p_vl_novo_salario,
        func_dt_alteracao = CURRENT_TIMESTAMP
    WHERE func_cd_funcionario = p_cd_funcionario;

    COMMIT;
END;
$$;

COMMENT ON PROCEDURE sp_a11732_atualiza_salario_upd(INTEGER, NUMERIC) IS 'Atualiza salário de funcionário';
```

---

### 11. Triggers

**Padrão:** `trg_<momento>_<mnemônico>_<operação>_<sistema>`

**Momentos:** `bf` (BEFORE), `af` (AFTER), `io` (INSTEAD OF)  
**Operações:** `ins`, `upd`, `del`, `iud` (INSERT/UPDATE/DELETE)

```sql
CREATE OR REPLACE FUNCTION fn_trg_af_func_iu()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.func_dt_inclusao := CURRENT_TIMESTAMP;
    END IF;

    IF TG_OP = 'UPDATE' THEN
        NEW.func_dt_alteracao := CURRENT_TIMESTAMP;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_af_func_iu_a11732
    AFTER INSERT OR UPDATE ON funcionario
    FOR EACH ROW
    EXECUTE FUNCTION fn_trg_af_func_iu();

COMMENT ON TRIGGER trg_af_func_iu_a11732 ON funcionario IS 'Atualiza timestamps de auditoria';
```

---

### 12. Materialized Views

**Padrão:** `mv_<padrão_tabela>`

```sql
CREATE MATERIALIZED VIEW mv_funcionario_ativo AS
SELECT
    func_cd_funcionario,
    func_nm_completo,
    func_dt_admissao
FROM funcionario
WHERE func_in_ativo = 'S';

CREATE UNIQUE INDEX in_mv_func_ativo_cd ON mv_funcionario_ativo (func_cd_funcionario);

COMMENT ON MATERIALIZED VIEW mv_funcionario_ativo IS 'Visão materializada de funcionários ativos';
```

---

## Template Completo de Tabela

```sql
-- ========================================
-- Tabela: funcionario
-- Descrição: Cadastro de funcionários
-- ========================================

CREATE TABLE funcionario (
    -- Chave primária (auto-incremento)
    func_cd_funcionario INTEGER GENERATED BY DEFAULT AS IDENTITY NOT NULL,

    -- Dados pessoais
    func_nm_completo VARCHAR(100) NOT NULL,
    func_nm_email VARCHAR(80),
    func_dt_nascimento DATE,
    func_dt_admissao DATE NOT NULL,

    -- Identificadores
    func_nr_cpf VARCHAR(11),
    func_nr_matricula INTEGER,

    -- Classificação
    func_cd_cargo INTEGER,
    func_cd_departamento INTEGER,

    -- Indicadores
    func_in_ativo CHAR(1) DEFAULT 'S',

    -- Valores
    func_vl_salario NUMERIC(10,2),
    func_pr_comissao NUMERIC(5,2),

    -- Quantidades
    func_qn_dependentes INTEGER,

    -- Texto livre
    func_tx_observacao TEXT,

    -- Controle (auditoria)
    func_dt_inclusao TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    func_dt_alteracao TIMESTAMP WITH TIME ZONE,

    -- Constraints
    CONSTRAINT pk_func PRIMARY KEY (func_cd_funcionario),
    CONSTRAINT un_func_cpf UNIQUE (func_nr_cpf),
    CONSTRAINT un_func_matricula UNIQUE (func_nr_matricula),
    CONSTRAINT ck_func_in_ativo CHECK (func_in_ativo IN ('S', 'N')),
    CONSTRAINT ck_func_vl_salario CHECK (func_vl_salario > 0),
    CONSTRAINT ck_func_pr_comissao CHECK (func_pr_comissao BETWEEN 0 AND 100),
    CONSTRAINT fk_carg_func_exercicio
        FOREIGN KEY (func_cd_cargo)
        REFERENCES cargo (carg_cd_cargo),
    CONSTRAINT fk_dept_func_lotacao
        FOREIGN KEY (func_cd_departamento)
        REFERENCES departamento (dept_cd_departamento)
);

-- Comentários obrigatórios
COMMENT ON TABLE funcionario IS 'Cadastro de funcionários da empresa';
COMMENT ON COLUMN funcionario.func_cd_funcionario IS 'Código único do funcionário';
COMMENT ON COLUMN funcionario.func_nm_completo IS 'Nome completo do funcionário';
COMMENT ON COLUMN funcionario.func_in_ativo IS 'Indicador se funcionário está ativo (S/N)';
COMMENT ON COLUMN funcionario.func_vl_salario IS 'Salário base do funcionário';

-- Índices
CREATE INDEX in_fk_carg_func_exercicio ON funcionario (func_cd_cargo);
CREATE INDEX in_fk_dept_func_lotacao ON funcionario (func_cd_departamento);
CREATE INDEX in_func_email ON funcionario (func_nm_email);
CREATE INDEX in_func_nome ON funcionario (func_nm_completo);

-- View de funcionários ativos
CREATE OR REPLACE VIEW vw_funcionario_ativo AS
SELECT
    func_cd_funcionario,
    func_nm_completo,
    func_nm_email,
    func_nr_matricula,
    func_dt_admissao,
    func_vl_salario
FROM funcionario
WHERE func_in_ativo = 'S';

-- Trigger de auditoria
CREATE OR REPLACE FUNCTION fn_trg_af_func_iu()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        NEW.func_dt_inclusao := CURRENT_TIMESTAMP;
    END IF;

    IF TG_OP = 'UPDATE' THEN
        NEW.func_dt_alteracao := CURRENT_TIMESTAMP;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_af_func_iu_a11732
    AFTER INSERT OR UPDATE ON funcionario
    FOR EACH ROW
    EXECUTE FUNCTION fn_trg_af_func_iu();
```

---

## Tipos de Dados Recomendados (PostgreSQL)

### Códigos/Identificadores com Auto-incremento

**✅ Recomendação Atual (PostgreSQL 10+): Identity Columns**

Utilize `INTEGER` ou `BIGINT` com `GENERATED BY DEFAULT AS IDENTITY` para chaves primárias auto-incrementadas:

```sql
-- Recomendado (padrão SQL:2003)
func_cd_funcionario INTEGER GENERATED BY DEFAULT AS IDENTITY NOT NULL
PROJ_CD_PROJETO BIGINT GENERATED BY DEFAULT AS IDENTITY NOT NULL

-- Alternativa legada (desencorajada, mas ainda funciona)
func_cd_funcionario SERIAL NOT NULL
```

**Justificativas para Identity Columns:**

1. **Padrão SQL:2003** - Sintaxe portável entre SGBDs (Oracle, SQL Server, PostgreSQL)
2. **Tipo explícito** - Deixa claro se é INTEGER ou BIGINT
3. **Menos objetos implícitos** - SERIAL cria sequências ocultas que podem causar problemas
4. **Melhor controle** - Permite configurações avançadas (START WITH, INCREMENT BY)
5. **Recomendação oficial PostgreSQL** - Desde a versão 10

**Referências:**

- [PostgreSQL 16 Documentation - Identity Columns](https://www.postgresql.org/docs/16/ddl-identity-columns.html)
- [PostgreSQL Wiki - Don't use SERIAL](https://wiki.postgresql.org/wiki/Don%27t_Do_This#Don.27t_use_serial)

**Variações de Identity:**

```sql
-- Permite inserção manual (padrão recomendado)
GENERATED BY DEFAULT AS IDENTITY

-- Proíbe inserção manual (mais restritivo)
GENERATED ALWAYS AS IDENTITY

-- Com opções customizadas
GENERATED BY DEFAULT AS IDENTITY (START WITH 1000 INCREMENT BY 1)
```

### Tipos Numéricos para Identificadores

```sql
-- Códigos/Identificadores
SMALLINT          -- Códigos pequenos (até 32.767)
INTEGER           -- Códigos médios (até 2.147.483.647) - RECOMENDADO
BIGINT            -- Códigos muito grandes (até 9.223.372.036.854.775.807)

-- Valores monetários
NUMERIC(15,2)     -- Até 999.999.999.999,99
NUMERIC(10,2)     -- Até 99.999.999,99
MONEY             -- Tipo nativo PostgreSQL

-- Percentuais e taxas
NUMERIC(5,2)      -- Até 999,99%
NUMERIC(3,2)      -- Até 9,99%

-- Quantidades e medidas
INTEGER           -- Quantidades inteiras
NUMERIC(10,3)     -- Medidas com 3 decimais
NUMERIC(15,6)     -- Coordenadas geográficas

-- Textos
CHAR(1)           -- Indicadores (S/N, A/I)
CHAR(2)           -- UF, códigos fixos
VARCHAR(11)       -- CPF, CNPJ
VARCHAR(50)       -- Nomes curtos
VARCHAR(100)      -- Nomes completos
VARCHAR(200)      -- Descrições
TEXT              -- Textos longos

-- Datas
DATE                        -- Data simples
TIMESTAMP                   -- Data com hora
TIMESTAMP WITH TIME ZONE    -- Data com fuso horário (recomendado)

-- Binários
BYTEA             -- Arquivos, imagens

-- Booleanos
BOOLEAN           -- Alternativa a CHAR(1) para indicadores

-- JSON
JSON              -- Dados JSON
JSONB             -- Dados JSON binário (recomendado, mais performático)
```

---

## Validações Comuns

```sql
-- Status e indicadores
CHECK (campo_in_status IN ('A', 'I', 'P', 'C'))  -- Ativo, Inativo, Pendente, Cancelado
CHECK (campo_in_ativo IN ('S', 'N'))             -- Sim, Não
CHECK (campo_in_sexo IN ('M', 'F'))              -- Masculino, Feminino

-- Ou usando BOOLEAN (PostgreSQL nativo)
CHECK (campo_in_ativo IS TRUE OR campo_in_ativo IS FALSE)

-- Valores monetários e numéricos
CHECK (campo_vl_valor > 0)                       -- Maior que zero
CHECK (campo_vl_valor >= 0)                      -- Maior ou igual a zero
CHECK (campo_pr_percentual BETWEEN 0 AND 100)    -- Percentuais

-- Datas
CHECK (campo_dt_fim >= campo_dt_inicio)          -- Data fim maior que início
CHECK (campo_dt_nascimento < CURRENT_DATE)       -- Data no passado
```

---

## Alternativas Consideradas

### Alternativa 1: Snake_case minúsculo (padrão PostgreSQL)

**Motivo da rejeição:** Embora seja o padrão de fato do PostgreSQL, optamos por MAIÚSCULAS para manter aderência aos padrões corporativos Petrobras e facilitar migração/integração com sistemas Oracle legados.

### Alternativa 2: Nomenclatura sem mnemônicos

**Motivo da rejeição:** Mnemônicos facilitam a identificação rápida de relacionamentos e origem de colunas em queries complexas, especialmente em JOINs.

### Alternativa 3: Prefixos `tbl_`, `idx_`, `vw_`

**Motivo da rejeição:** Redundantes e verbosos. O tipo de objeto já é identificado pelo contexto de uso e pelo padrão de nomenclatura específico (PK*, FK*, etc).

---

## Consequências

### Positivas ✅

- ✅ **Aderência aos padrões corporativos** Petrobras (PE-2TIC-00319)
- ✅ **Rastreabilidade completa** através de nomenclatura padronizada
- ✅ **Facilita onboarding** de novos desenvolvedores
- ✅ **Compatibilidade** com ferramentas de análise e auditoria
- ✅ **Documentação automatizada** através de comentários obrigatórios
- ✅ **Queries mais legíveis** através de mnemônicos consistentes
- ✅ **Facilita migração/integração** com sistemas Oracle legados
- ✅ **Reduz erros** através de convenções claras

### Negativas ⚠️

- ⚠️ **Nomes mais longos** devido a mnemônicos e códigos de classe
- ⚠️ **Curva de aprendizado** inicial para equipe não familiarizada
- ⚠️ **Diferença do padrão PostgreSQL** (snake_case minúsculo)
- ⚠️ **Necessidade de disciplina** para manter consistência

### Neutras ℹ️

- ℹ️ Requer **configuração de ferramentas de qualidade** (linters, validadores)
- ℹ️ Necessita **treinamento da equipe** nos padrões adotados
- ℹ️ Demanda **revisão de código rigorosa** para garantir aderência

---

## Conformidade e Validação

### Ferramentas de Validação

- **pgFormatter**: Formatação padronizada de SQL
- **SQLFluff**: Linting de SQL
- **Custom scripts**: Validação de nomenclatura via regex

### Processo de Revisão

1. Todo script SQL deve ser revisado quanto a aderência aos padrões
2. CI/CD deve incluir validação automatizada de nomenclatura
3. Code review deve verificar:
   - ✅ Nomenclatura de objetos
   - ✅ Presença de comentários
   - ✅ Uso correto de tipos de dados
   - ✅ Implementação de constraints

---

## Referências

- **Norma Petrobras:** PE-2TIC-00319 - Padrões de Nomenclatura de Objetos de BD
- **Base de Conhecimento:** `@petrobrasbr-forge/dsenge-kairos-agentes/bases-conhecimento/petrobras-sql-patterns.md`
- **Template ADR:** `@petrobrasbr-forge/dsenge-kairos-agentes/templates/001.02-template-decisao-arquitetural.md`
- **Guia de Documentação:** `@petrobrasbr-forge/dsenge-kairos-agentes/guias/001-guia-documentacao-arquitetura.md`
- **PostgreSQL Naming Conventions:** https://www.postgresql.org/docs/current/sql-syntax-lexical.html

---

## Histórico de Revisões

| Data       | Versão | Autor         | Descrição                                  |
| ---------- | ------ | ------------- | ------------------------------------------ |
| 2026-01-23 | 1.0    | Líder Técnico | Versão inicial - Padrões PostgreSQL a11732 |

---

## Aprovações

| Papel                | Nome       | Data       | Assinatura |
| -------------------- | ---------- | ---------- | ---------- |
| Líder Técnico        | [Pendente] | [Pendente] |            |
| Arquiteto de Solução | [Pendente] | [Pendente] |            |
| Product Owner        | [Pendente] | [Pendente] |            |

---

## Anexos

### Anexo A: Checklist de Validação de Scripts SQL

```markdown
- [ ] Nomes de tabelas em minúsculas, singular, masculino
- [ ] Nomes de colunas seguem padrão mnem_classe_descrição (minúsculas)
- [ ] Mnemônicos de tabela respeitam regra de formação (até 4 caracteres)
- [ ] Códigos de classe corretos (cd, ds, vl, in, dt, nm, nr, etc)
- [ ] Constraints nomeadas conforme padrão (pk**, fk\*\_, un\_\_, ck**, df\*\*) em minúsculas
- [ ] Índices nomeados conforme padrão (in**, in_fk**) em minúsculas
- [ ] Comentários presentes em todas as tabelas
- [ ] Comentários presentes em todas as colunas
- [ ] Tipos de dados adequados e consistentes
- [ ] Validações (CHECK constraints) implementadas quando necessário
- [ ] Defaults configurados onde aplicável
- [ ] Views/Functions/Procedures seguem nomenclatura padrão em minúsculas
```

### Anexo B: Scripts de Validação

```sql
-- Script para validar nomenclatura de tabelas
SELECT
    SCHEMANAME,
    TABLENAME,
    CASE
        WHEN TABLENAME !~ '^[A-Z][A-Z0-9_]*$' THEN 'Nome contém caracteres inválidos'
        WHEN TABLENAME ~ '.*S$' THEN 'Nome está no plural (possivelmente)'
        WHEN LENGTH(TABLENAME) > 30 THEN 'Nome muito longo (>30 caracteres)'
        ELSE 'OK'
    END AS VALIDACAO
FROM PG_TABLES
WHERE SCHEMANAME = 'a11732'
AND TABLENAME !~ '^(pg_|sql_)';

-- Script para validar comentários obrigatórios
SELECT
    T.SCHEMANAME,
    T.TABLENAME,
    CASE
        WHEN D.DESCRIPTION IS NULL THEN 'Comentário ausente na tabela'
        ELSE 'OK'
    END AS VALIDACAO_TABELA
FROM PG_TABLES T
LEFT JOIN PG_DESCRIPTION D ON D.OBJOID = (T.SCHEMANAME||'.'||T.TABLENAME)::REGCLASS
WHERE T.SCHEMANAME = 'a11732';
```

---

### Anexo C: Lista de Verificação de Corretude do Modelo de Dados
