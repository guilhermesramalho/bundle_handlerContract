# Plano de Implementação — API de Contrato

> Detalha **como** executar a Fase 1 (`GrupoEconomico` mínimo → `Contrato` →
> endpoints de listagem/filtro) de `PLANO-IMPLEMENTACAO.md`. Este documento é
> a expansão detalhada das seções 1.1, 1.3 e 1.4 daquele plano — mesma
> relação que `PLANO-IMPLEMENTACAO-FRONTEND.md` tem com a Fase 2. Fonte de
> verdade de negócio continua sendo `context/CONTEXTO-COMPLETO-PROJETO.md`.
>
> **Fora de escopo deste documento** (não implementar aqui, ver motivo em
> cada seção): `GuardaChuva` como aggregate completo (Fase 1.2, bloqueada —
> modelado como valor embutido em `Contrato`, ver seção 3), aba Jurídico/Elaw
> (skill `integracao-elaw`), Performance/Reappraise/PIR (E2/E3, Fase 6),
> integração SAP/PCR real (Fase 5).

---

## 0. Nomenclatura da tabela (decisão já tomada)

A tabela chama-se **`Contrato`** (não `ClienteContrato`) — segue o DT-003
(PascalCase, singular, sem underscore/prefixo), o mesmo padrão já aplicado em
`Cliente`. `Contrato.IdCliente` é FK para `Cliente` (que já existe). Uma
tabela `ClienteContrato` só faria sentido para uma associação N:N, que não é
o caso (1 `Cliente`/CNPJ pode ter N `Contrato`s ao longo do tempo, mas cada
`Contrato` pertence a exatamente 1 `Cliente`).

---

## 1. Pré-requisito: `GrupoEconomico` mínimo (Fase 1.1 do plano mestre)

`Contrato` precisa agrupar por grupo econômico (usado nos filtros/coluna
"Grupo econômico" da Listagem, F2 do frontend já implementada). Construir
antes de `Contrato`, conforme a ordem já definida em `PLANO-IMPLEMENTACAO.md`
seção 1.1 ("menor risco — começar por aqui").

### 1.1 Entidade

```
PortalAle.Domain/GruposEconomicos/GrupoEconomico.cs
PortalAle.Domain/GruposEconomicos/IGrupoEconomicoRepository.cs
```

Campos (mínimo, cadastro manual — ver nota de escopo abaixo):

| Propriedade | Tipo | Coluna (DT-003) | Obrigatório | Observação |
|---|---|---|---|---|
| `Id` | `int` | `IdGrupoEconomico` | — | herdado de `Entity` |
| `Codigo` | `string` | `CdSap` | Sim, único | Código SAP de 3 dígitos (seção 9 do contexto) |
| `Nome` | `string` | `NmGrupo` | Sim | Ex.: "Grupo Solaris" |
| `DataCriacao`/`DataAlteracao` | `DateTimeOffset`/`?` | `DhInclusao`/`DhAlteracao` | Sim/Não | Auditoria de linha, mesmo padrão de `Cliente` |

Validação de domínio: `Codigo` deve ter exatamente 3 dígitos numéricos
(`DomainException` caso contrário) — ver seção 9 do contexto.

> ⚠️ **Nota de escopo (já registrada em `PLANO-IMPLEMENTACAO.md` 1.1):** no
> desenho-alvo, `GrupoEconomico` vem do SAP. Como a integração SAP está
> bloqueada (Fase 5), isto aqui é cadastro manual — **placeholder
> consciente**, registrar no PR. Não implementar `Atualizar`/`Excluir` ainda
> (grupo econômico não deveria ser editado livremente uma vez que a
> integração SAP chegar) — só `Criar`, `Consultar`, `Listar`.

### 1.2 Migration complementar em `Cliente`

`Cliente` ganha 3 colunas novas (migration de **alteração**, não recriar a
tabela — ela já tem dados/testes):

| Coluna nova | Tipo | Motivo |
|---|---|---|
| `IdGrupoEconomico` (nullable) | `int` FK → `GrupoEconomico` | Já previsto em `PLANO-IMPLEMENTACAO.md` 1.1 |
| `NrSap` (nullable, 10 dígitos) | `varchar(10)` | Seção 9 do contexto: "Número SAP do cliente (10 dígitos) identifica o **cliente**" — hoje `Cliente` não tem esse campo; é dele, não de `Contrato` |
| `SgUf` (nullable, 2 chars) | `char(2)` | UF do CNPJ/ponto de venda — hoje `Cliente.Endereco` é texto livre sem UF estruturada, e a Listagem (F2) filtra/exibe UF. Adicionar estruturado evita repetir o hack do protótipo (`CITY_UF`, um mapa hardcoded nome-de-cidade → UF) |

**Por que em `Cliente` e não em `Contrato`:** `NrSap` e UF identificam o
CNPJ/pessoa jurídica, não o contrato — um mesmo `Cliente` poderia ter mais de
um `Contrato` ao longo do tempo (histórico de renovações) com o mesmo SAP/UF.
Duplicar em `Contrato` seria desnormalização sem motivo. Mantém `Contrato`
focado em termos contratuais.

Não é objeto deste plano detalhar `CriarClienteCommandHandler`/etc. — só
adicionar as colunas e os 3 campos na entidade `Cliente` existente (setters
privados, sem quebrar o construtor rico atual: usar sobrecarga ou método
`AtribuirDadosComplementares(...)`, já que `Cliente` já está em produção
lógica com CNPJ obrigatório desde a criação).

---

## 2. Inventário de campos do protótipo → decisão de modelagem

Fonte: `docs/modelo-frontend/_extraido/template.html` (`build()`/`process()`
da classe `Component extends DCLogic`) e o que já foi portado em
`frontend/src/mocks/contratos.ts` (Fase F2 do frontend, concluída). Cada
campo do protótipo foi classificado em uma de 4 categorias:

- **P** = Persistir em `Contrato` (coluna real)
- **C** = Calculado em runtime (domínio ou query), **não** persistir
- **Cliente** = Pertence a `Cliente`, não a `Contrato` (ver seção 1.2)
- **Fora** = Fora de escopo deste plano (outro módulo/integração) — não
  implementar, sinalizar

| Campo do protótipo | Categoria | Coluna/decisão | Motivo |
|---|---|---|---|
| `id` | P | `IdContrato` (Identity) | PK |
| `pcr` / `pcf` | P | `NrPcr` (único) | Seção 8 do contexto: **um único campo** com prefixo PCR/PCF conforme segmento (Rede vs. B2B), não dois campos separados — `IsB2B(Segmento)` decide o prefixo na apresentação, não persistir os dois |
| `cnpj` | Cliente | já existe (`Cliente.Cnpj`) | — |
| `razao` | Cliente | já existe (`Cliente.Nome`) | — |
| `segmento` | P | `DsSegmento` (enum `Segmento`) | Rede/GRR/COFA/COFD/COFAR/Outros/Spot — ver nota "Posa" abaixo |
| `grupo` | Cliente | via `Cliente.GrupoEconomico` (seção 1) | — |
| `diretoria`, `gr`, `rn`, `consultor` | P | `DsDiretoria`, `DsRegionalVendas`, `DsPontoVenda`, `NmConsultor` (texto livre) | Hierarquia comercial — **não** normalizar em tabela de estrutura organizacional agora: a origem desses dados (SAP vs. cadastro manual) é decisão em aberto (seção 2 do `PLANO-IMPLEMENTACAO-FRONTEND.md`, seção 14 do contexto). Persistir como snapshot textual no próprio contrato evita assumir a resposta |
| `situacaoMes` | P | `DsSituacaoMes` (enum `SituacaoMes`: Ativo/Inativo) | Status vindo do SICOF/PCR |
| `tipo` | P | `DsTipoContrato` (enum `TipoContrato`) | PCVM/Imagem/Comodato |
| `bandeira` | P | `DsBandeira` (texto) | ⚠️ mecanismo de atualização automática é item em aberto (seção 14 do contexto) — persistir como campo editável manualmente por ora |
| `registradoALE` | P | `InRegistradoAle` (bit) | — |
| `dataANP` | P | `DtRegistroAnp` (date, nullable) | — |
| `inicioBR` / `fimBR` | P | `DtInicioVigencia` / `DtFimVigencia` (date) | — |
| `volMensal` | P | `MdVolumeMensalContratado` (decimal) | Base para `GalonagemContratada` (ver domínio, seção 4) |
| `frac` | **Fora** (mas ver `MdGalonagemFaturada`) | — | No protótipo é um fator mock (`0..1`) só para simular faturamento. No sistema real, volume faturado vem da integração SAP/PCR (Fase 5) — ver campo `MdGalonagemFaturada` abaixo, que existe na tabela mas só é preenchido quando a integração existir |
| `perf` | Fora | — | Usado só para simular a série mensal no protótipo (gráficos de Performance, E2) — não é dado de `Contrato`, é resultado calculado de faturamento real vs. contratado mês a mês. Pertence à Fase 6 (E2 Performance) |
| `margemBase` | P | `PrMargemBase` (decimal) | Taxa de margem — usada por TIR/Performance |
| `greenfield` | P | `InGreenfield` (bit) | — |
| `denuncia` | P | `InDenuncia` (bit) | — |
| `galVencido` | **não portar** | — | Só existe no dataset mock como *override* manual para forçar um cenário de teste (contrato #8) — não é um campo real do domínio; a situação real vem 100% do cálculo (seção 4) |
| `umbrella.role` / `umbrella.group` | P | `TpPapelGuardaChuva` (enum nullable: Principal/Adicional), `CdGuardaChuva` (texto nullable) | Guarda-chuva completo (Fase 1.2) está bloqueado por uma pergunta ao cliente ("CNPJ pode ter mais de uma PCR ativa simultaneamente?"). Embutir só o suficiente para não travar `Contrato` — não criar a tabela/aggregate `GuardaChuva` agora |
| `obs` | P | `TxObservacao` (nvarchar(max), nullable) | — |
| `clausula` | P | `TxClausula` (nvarchar(max), nullable) | — |
| `sucedidoCnpj` / `sucedidoRazao` | P | `NrCnpjSucedido`, `NmRazaoSucedido` (nullable) | Descritivo do contrato/CNPJ predecessor em caso de Sucessão — não é FK (o predecessor pode nem existir mais como `Cliente` ativo) |
| `juridico.*`, `acoes[]` | **Fora** | — | Aba Jurídico — dado vem do Elaw (skill `integracao-elaw`), não é escrito/gerenciado por este agregado. `Contrato` não deve ter uma tabela de ações jurídicas própria |
| `tir.proj` | **Fora** | — | "TIR Projetada" — o próprio contexto (seção 10) confirma que **não pode ser calculada com o que o sistema tem hoje** |
| `tir.real` | **Fora** | — | "TIR Realizada" depende de fluxo de caixa acumulado (Performance, E2/Fase 6), não é dado de criação do contrato |
| *(implícito)* TIR Contratada | P | `PrTirContratada` (decimal, nullable) | Única das 3 TIRs que é fixa na aprovação do contrato (seção 10 do contexto) — só essa persiste em `Contrato` |
| `eventos[]` | P (tabela filha) | `HistoricoEventoContrato` | Ver seção 4 — enum fechado de 7 tipos, já confirmado na seção 9 do contexto e em `PLANO-IMPLEMENTACAO.md` 1.3 |
| `garantia` | P | `InGarantia` (bit) | — |
| `sublocado` | P | `InSublocado` (bit) | — |
| `sublocacaoPrazo` | Fora | — | No protótipo é uma data **inventada aleatoriamente** (`rnd()`) — não existe fonte real no dataset mock; não inventar coluna para um dado que nem o protótipo tem de verdade |
| `encerrado` | P | `InEncerrado` (bit, default 0) | Mesmo padrão de soft-flag que `Cliente.Ativo`, mutado por `Contrato.Encerrar()` (ver seção 4) |
| *(derivado)* `contratoId` (prefixo PCR/PCF) | C | — | `(IsB2B(Segmento) ? "PCF " : "PCR ") + NrPcr`, calculado na apresentação |
| *(derivado)* `uf`, `sapCliente` | Cliente | via `Cliente.SgUf`/`Cliente.NrSap` | Ver seção 1.2 |
| *(derivado)* `galContratada`, `galFaturada`, `saldo`, `galPct`, `situacaoPcr`, cores de barra/chip, `projVencGalDate` | C | método `Contrato.CalcularGalonagem(DateTimeOffset referencia)` | Ver seção 4 — regra de negócio fechada (seção 9 do contexto), não persistir estado derivável |
| *(derivado)* `diretor`, `gerExec`, `regional`, `gerenteRegional`, `coordenador`, `supervisor`, `representante` | **não portar** | — | No protótipo vêm de um `Dictionary` hardcoded (`HIER_REG`/`HIER_GR`) simulando a estrutura organizacional da ALE. Como a origem real é decisão em aberto (mesma pendência do campo `gr`/`diretoria` acima), não recriar esse mapeamento fixo no backend — os 4 campos textuais já persistidos (`DsDiretoria`, `DsRegionalVendas`, etc.) bastam para os filtros da Listagem hoje |

**Sobre `MdGalonagemFaturada`:** mesmo com `frac`/`perf` fora de escopo, a
regra de "situação da PCR" (seção 9 do contexto) depende de **quanto já foi
faturado** vs. contratado. Sem isso, `Contrato` nunca teria como calcular
`situacaoPcr` corretamente. Decisão: persistir `MdGalonagemFaturada` (decimal,
default `0`) na tabela `Contrato`, **não** calculado a partir de `frac`/`perf`
(que são artefatos só do dataset mock) — é um campo que só a integração
SAP/PCR (Fase 5) vai manter atualizado de verdade. Até lá, todo contrato novo
nasce com `0` e a situação computada será sempre "Vigente" ou "Vencido por
data" (nunca "Vencido por galonagem") — **consequência aceitável e
documentada**, não um bug.

**Nota — segmento "Posa":** o dataset mock do protótipo usa um segmento
`"Posa"` (contrato #10, "Frota Rápida Spot Ltda" — na verdade usa `"Spot"`,
`Posa` aparece só nos mapas `SEGTONE`/`DEFAULT_MIX`, sem contrato de exemplo
usando-o). A lista oficial de segmentos (seção 9 do contexto) é **Rede / GRR
/ COFA / COFD / COFAR / Outros / Spot** — `Posa` não está nela. Não incluir
`Posa` no enum `Segmento`; se aparecer depois, tratar como gap a esclarecer
com o negócio, não copiar do protótipo por inércia.

---

## 3. Modelo de domínio

### 3.1 Enums (`PortalAle.Domain/Contratos/`)

```csharp
public enum Segmento { Rede, Grr, Cofa, Cofd, Cofar, Outros, Spot }

public enum TipoContrato { Pcvm, Imagem, Comodato }

public enum SituacaoMes { Ativo, Inativo }

public enum PapelGuardaChuva { Principal, Adicional }

/// <summary>Únicos 7 tipos válidos de evento no ciclo de vida do contrato — seção 9 do contexto.</summary>
public enum TipoEventoContrato { NovoNegocio, Renovacao, Readequacao, Cessao, Sucessao, Denuncia, Encerramento }

/// <summary>Resultado do cálculo de galonagem — nunca persistido, sempre calculado. Ver Contrato.CalcularGalonagem().</summary>
public enum SituacaoPcr { Vigente, VencidoPorGalonagem, VencidoPorData }
```

### 3.2 `Contrato` (Aggregate Root)

```csharp
namespace PortalAle.Domain.Contratos;

public class Contrato : Entity
{
    public int ClienteId { get; private set; }
    public virtual Cliente? Cliente { get; private set; }

    public string Pcr { get; private set; } = string.Empty;
    public Segmento Segmento { get; private set; }
    public TipoContrato Tipo { get; private set; }
    public SituacaoMes SituacaoMes { get; private set; }
    public string Bandeira { get; private set; } = string.Empty;
    public bool RegistradoAle { get; private set; }
    public DateOnly? DataRegistroAnp { get; private set; }

    public DateOnly InicioVigencia { get; private set; }
    public DateOnly FimVigencia { get; private set; }

    public decimal VolumeMensalContratado { get; private set; }
    public decimal GalonagemFaturada { get; private set; }
    public decimal MargemBase { get; private set; }
    public decimal? TirContratada { get; private set; }

    public bool Greenfield { get; private set; }
    public bool Denuncia { get; private set; }
    public bool Garantia { get; private set; }
    public bool Sublocado { get; private set; }
    public bool Encerrado { get; private set; }

    // Guarda-chuva — versão embutida enquanto GuardaChuva (Fase 1.2) está bloqueado.
    public PapelGuardaChuva? PapelGuardaChuva { get; private set; }
    public string? CodigoGuardaChuva { get; private set; }

    // Hierarquia comercial — snapshot textual, ver seção 2 (origem em aberto).
    public string Diretoria { get; private set; } = string.Empty;
    public string RegionalVendas { get; private set; } = string.Empty;
    public string PontoVenda { get; private set; } = string.Empty;
    public string Consultor { get; private set; } = string.Empty;

    public string? Observacao { get; private set; }
    public string? Clausula { get; private set; }
    public string? CnpjSucedido { get; private set; }
    public string? RazaoSucedido { get; private set; }

    public DateTimeOffset DataCriacao { get; private set; }
    public DateTimeOffset? DataAlteracao { get; private set; }

    public virtual ICollection<EventoContrato> Eventos { get; private set; } = new List<EventoContrato>();

    public Contrato() { }
    public Contrato(int id) : base(id) { }

    public Contrato(
        int clienteId, string pcr, Segmento segmento, TipoContrato tipo, SituacaoMes situacaoMes,
        string bandeira, bool registradoAle, DateOnly inicioVigencia, DateOnly fimVigencia,
        decimal volumeMensalContratado, decimal margemBase, string diretoria, string regionalVendas,
        string pontoVenda, string consultor, bool greenfield = false)
    {
        ValidarPcr(pcr);
        ValidarVigencia(inicioVigencia, fimVigencia);
        ValidarVolume(volumeMensalContratado);

        ClienteId = clienteId;
        Pcr = pcr.Trim();
        Segmento = segmento;
        Tipo = tipo;
        SituacaoMes = situacaoMes;
        Bandeira = bandeira.Trim();
        RegistradoAle = registradoAle;
        InicioVigencia = inicioVigencia;
        FimVigencia = fimVigencia;
        VolumeMensalContratado = volumeMensalContratado;
        MargemBase = margemBase;
        Diretoria = diretoria.Trim();
        RegionalVendas = regionalVendas.Trim();
        PontoVenda = pontoVenda.Trim();
        Consultor = consultor.Trim();
        Greenfield = greenfield;
        GalonagemFaturada = 0; // até a integração SAP/PCR existir (Fase 5) — ver seção 2.
        DataCriacao = DateTimeOffset.UtcNow;

        RegistrarEvento(TipoEventoContrato.NovoNegocio, DateOnly.FromDateTime(DateTime.UtcNow), "Assinatura do contrato.");
    }

    /// <summary>
    /// Regra de negócio fechada (contexto seção 9). Nunca persistir o resultado —
    /// recalcular sempre contra a data de referência (normalmente "hoje").
    /// </summary>
    public (SituacaoPcr Situacao, decimal GalonagemContratada, decimal Saldo, decimal PercentualCumprido) CalcularGalonagem(DateOnly referencia)
    {
        int mesesVigencia = Math.Max(1, MesesEntre(InicioVigencia, FimVigencia));
        decimal galonagemContratada = VolumeMensalContratado * mesesVigencia;
        decimal saldo = Math.Max(0, galonagemContratada - GalonagemFaturada);
        decimal percentual = galonagemContratada == 0 ? 0 : GalonagemFaturada / galonagemContratada;

        bool cumpriuGalonagem = percentual >= 1 || saldo <= 0;
        bool prazoExpirado = FimVigencia < referencia;

        SituacaoPcr situacao = cumpriuGalonagem
            ? SituacaoPcr.VencidoPorGalonagem
            : prazoExpirado
                ? SituacaoPcr.VencidoPorData
                : SituacaoPcr.Vigente;

        return (situacao, galonagemContratada, saldo, percentual);
    }

    public void RegistrarEvento(TipoEventoContrato tipo, DateOnly data, string descricao)
    {
        if (Encerrado)
            throw new DomainException("Não é possível registrar evento em contrato encerrado.");

        Eventos.Add(new EventoContrato(Id, tipo, data, descricao));

        if (tipo == TipoEventoContrato.Encerramento)
            Encerrado = true;

        if (tipo == TipoEventoContrato.Denuncia)
            Denuncia = true;

        DataAlteracao = DateTimeOffset.UtcNow;
    }

    public void AtualizarGalonagemFaturada(decimal novaGalonagemFaturada)
    {
        if (novaGalonagemFaturada < 0)
            throw new DomainException("GalonagemFaturada: não pode ser negativa.");

        GalonagemFaturada = novaGalonagemFaturada;
        DataAlteracao = DateTimeOffset.UtcNow;
    }

    private static void ValidarPcr(string pcr)
    {
        if (string.IsNullOrWhiteSpace(pcr))
            throw new DomainException("Pcr: O campo PCR/PCF é obrigatório");
    }

    private static void ValidarVigencia(DateOnly inicio, DateOnly fim)
    {
        if (fim < inicio)
            throw new DomainException("FimVigencia: A data de fim deve ser posterior ao início");
    }

    private static void ValidarVolume(decimal volume)
    {
        if (volume <= 0)
            throw new DomainException("VolumeMensalContratado: Deve ser maior que zero");
    }

    private static int MesesEntre(DateOnly a, DateOnly b)
        => (b.Year - a.Year) * 12 + (b.Month - a.Month);
}
```

**Notas de design (DT-022):**
- `Pcr` guarda só o número (`"700.123"`); o prefixo `PCR `/`PCF ` é
  apresentação (`IsB2B(Segmento)`), calculado num método de extensão ou na
  Query — não persistido, evita duplicidade com `Segmento`.
- `CalcularGalonagem` recebe a data de referência como parâmetro (não usa
  `DateTime.Now` internamente) — torna o método **testável
  deterministicamente** (DT-008/DT-022 exigem teste unitário de toda regra de
  negócio).
- `RegistrarEvento` é o único jeito de adicionar a `EventoContrato` — não
  expor `Eventos` como coleção mutável de fora (`ICollection` com `private
  set`, já no template).
- **Sem `Atualizar(...)` genérico tipo `Cliente.Atualizar`:** os campos de um
  contrato mudam por eventos de negócio específicos (`RegistrarEvento`,
  `AtualizarGalonagemFaturada`), não por um PUT genérico que reescreve tudo —
  intencional, contratos têm ciclo de vida mais rico que cadastro de cliente.
  Adicionar métodos de mutação pontuais (`AlterarBandeira`,
  `MarcarGarantia`, etc.) conforme casos de uso reais surgirem — não
  pré-criar setters para campos sem caso de uso claro ainda.

### 3.3 `EventoContrato` (entidade filha — **sem repositório próprio**, DT-018)

```csharp
namespace PortalAle.Domain.Contratos;

public class EventoContrato : Entity
{
    public int ContratoId { get; private set; }
    public virtual Contrato? Contrato { get; private set; }

    public TipoEventoContrato Tipo { get; private set; }
    public DateOnly Data { get; private set; }
    public string Descricao { get; private set; } = string.Empty;

    public EventoContrato() { }
    public EventoContrato(int id) : base(id) { }

    internal EventoContrato(int contratoId, TipoEventoContrato tipo, DateOnly data, string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descricao: O campo Descrição é obrigatório");

        ContratoId = contratoId;
        Tipo = tipo;
        Data = data;
        Descricao = descricao.Trim();
    }
}
```

Construtor `internal` — só `Contrato.RegistrarEvento` pode criar um
`EventoContrato`, reforçando que é entidade filha sem ciclo de vida próprio
(DT-018, seção "Repository APENAS para Aggregate Roots").

### 3.4 `IContratoRepository`

```csharp
namespace PortalAle.Domain.Contratos;

public interface IContratoRepository : IRepository<Contrato>
{
    Task<bool> ExistePorPcrAsync(string pcr, int? idParaIgnorar = null, CancellationToken cancellationToken = default);

    /// <summary>Carrega o contrato com os eventos (Include) — uso do Detalhe (F3, futuro) e de RegistrarEvento.</summary>
    Task<Contrato?> ObterComEventosAsync(int id, CancellationToken cancellationToken = default);
}
```

---

## 4. Persistência (EF Core / SQL Server)

### 4.1 DDL (`Contrato`)

```sql
CREATE TABLE Contrato (
    IdContrato INT IDENTITY(1,1) NOT NULL,
    IdCliente INT NOT NULL,

    NrPcr VARCHAR(20) NOT NULL,
    DsSegmento VARCHAR(20) NOT NULL,
    DsTipoContrato VARCHAR(20) NOT NULL,
    DsSituacaoMes VARCHAR(10) NOT NULL,
    DsBandeira NVARCHAR(60) NOT NULL,
    InRegistradoAle BIT NOT NULL DEFAULT 0,
    DtRegistroAnp DATE NULL,

    DtInicioVigencia DATE NOT NULL,
    DtFimVigencia DATE NOT NULL,

    MdVolumeMensalContratado DECIMAL(12,3) NOT NULL,
    MdGalonagemFaturada DECIMAL(15,3) NOT NULL DEFAULT 0,
    PrMargemBase DECIMAL(5,2) NOT NULL,
    PrTirContratada DECIMAL(5,2) NULL,

    InGreenfield BIT NOT NULL DEFAULT 0,
    InDenuncia BIT NOT NULL DEFAULT 0,
    InGarantia BIT NOT NULL DEFAULT 0,
    InSublocado BIT NOT NULL DEFAULT 0,
    InEncerrado BIT NOT NULL DEFAULT 0,

    DsPapelGuardaChuva VARCHAR(10) NULL,
    CdGuardaChuva VARCHAR(20) NULL,

    DsDiretoria NVARCHAR(60) NOT NULL,
    DsRegionalVendas NVARCHAR(60) NOT NULL,
    DsPontoVenda NVARCHAR(60) NOT NULL,
    NmConsultor NVARCHAR(100) NOT NULL,

    TxObservacao NVARCHAR(MAX) NULL,
    TxClausula NVARCHAR(MAX) NULL,
    NrCnpjSucedido VARCHAR(14) NULL,
    NmRazaoSucedido NVARCHAR(200) NULL,

    DhInclusao DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    DhAlteracao DATETIMEOFFSET NULL,

    CONSTRAINT PK_Contrato PRIMARY KEY (IdContrato),
    CONSTRAINT UQ_Contrato_NrPcr UNIQUE (NrPcr),
    CONSTRAINT CK_Contrato_DtVigencia CHECK (DtFimVigencia >= DtInicioVigencia),
    CONSTRAINT CK_Contrato_MdVolumeMensalContratado CHECK (MdVolumeMensalContratado > 0),
    CONSTRAINT FK_Contrato_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente (IdCliente)
);
GO

CREATE INDEX IX_Contrato_IdCliente ON Contrato (IdCliente);
CREATE INDEX IX_Contrato_DsSegmento ON Contrato (DsSegmento);
CREATE INDEX IX_Contrato_DtFimVigencia ON Contrato (DtFimVigencia);
GO

CREATE TABLE HistoricoEventoContrato (
    IdHistoricoEventoContrato INT IDENTITY(1,1) NOT NULL,
    IdContrato INT NOT NULL,
    DsTipoEvento VARCHAR(20) NOT NULL,
    DtEvento DATE NOT NULL,
    TxDescricao NVARCHAR(500) NOT NULL,

    CONSTRAINT PK_HistoricoEventoContrato PRIMARY KEY (IdHistoricoEventoContrato),
    CONSTRAINT FK_HistoricoEventoContrato_Contrato
        FOREIGN KEY (IdContrato) REFERENCES Contrato (IdContrato)
);
GO

CREATE INDEX IX_HistoricoEventoContrato_IdContrato ON HistoricoEventoContrato (IdContrato);
GO
```

Índices cobrem exatamente o que `PLANO-IMPLEMENTACAO.md` 1.3 pede ("CNPJ,
PCR/PCF, segmento, data de vencimento") — CNPJ já indexado via
`UQ_Cliente_NrCnpj` em `Cliente`, `NrPcr` via `UQ_Contrato_NrPcr`.

> Extended properties (`MS_Description`, DT-003) omitidas aqui por espaço —
> replicar o padrão de `ClienteConfiguration`/migration `CriarTabelaCliente`
> ao gerar o script real (toda tabela/coluna documentada).

### 4.2 `ContratoConfiguration` (segue `ClienteConfiguration` linha a linha)

```csharp
public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("Contrato", t => t.HasComment("Base contratual consolidada (Rede/GRR/B2B)"));

        builder.HasKey(c => c.Id).HasName("PK_Contrato");
        builder.Property(c => c.Id).HasColumnName("IdContrato").ValueGeneratedOnAdd().IsRequired();

        builder.Property(c => c.ClienteId).HasColumnName("IdCliente").IsRequired();

        builder.Property(c => c.Pcr).HasColumnName("NrPcr").HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();

        builder.Property(c => c.Segmento)
            .HasColumnName("DsSegmento")
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .IsRequired();

        builder.Property(c => c.Tipo).HasColumnName("DsTipoContrato").HasConversion<string>().HasColumnType("varchar(20)").IsRequired();
        builder.Property(c => c.SituacaoMes).HasColumnName("DsSituacaoMes").HasConversion<string>().HasColumnType("varchar(10)").IsRequired();

        builder.Property(c => c.VolumeMensalContratado).HasColumnName("MdVolumeMensalContratado").HasColumnType("decimal(12,3)").IsRequired();
        builder.Property(c => c.GalonagemFaturada).HasColumnName("MdGalonagemFaturada").HasColumnType("decimal(15,3)").IsRequired().HasDefaultValue(0);

        // ... demais propriedades seguem o mesmo padrão (HasColumnName + HasColumnType + IsRequired/(false) + HasComment) ...

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .HasConstraintName("FK_Contrato_Cliente")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Eventos)
            .WithOne(e => e.Contrato)
            .HasForeignKey(e => e.ContratoId)
            .HasConstraintName("FK_HistoricoEventoContrato_Contrato")
            .OnDelete(DeleteBehavior.Cascade); // eventos são parte do agregado — apagam junto (não há Excluir de Contrato hoje, mas mantém consistente)

        builder.HasIndex(c => c.ClienteId).HasDatabaseName("IX_Contrato_IdCliente");
        builder.HasIndex(c => c.Segmento).HasDatabaseName("IX_Contrato_DsSegmento");
        builder.HasIndex(c => c.FimVigencia).HasDatabaseName("IX_Contrato_DtFimVigencia");
        builder.HasIndex(c => c.Pcr).HasDatabaseName("UQ_Contrato_NrPcr").IsUnique();
    }
}
```

`EventoContratoConfiguration` análoga, tabela `HistoricoEventoContrato`, sem
`HasQueryFilter` (não tem soft delete).

Registrar `DbSet<Contrato> Contratos` (e opcionalmente
`DbSet<EventoContrato> HistoricoEventosContrato`, se precisar consultar
direto) em `ApplicationDbContext` — `EventoContrato` normalmente só é
acessado via `Contrato.Eventos` (`Include`), não precisa de `DbSet` próprio
por regra do DT-018 (sem repositório de entidade filha).

### 4.3 Migration

`dotnet ef migrations add CriarTabelaGrupoEconomicoEAlterarCliente` (seção 1)
e depois `dotnet ef migrations add CriarTabelaContrato` (seção 4) — duas
migrations separadas, cada uma no commit da fase correspondente (não
misturar `Cliente`+`GrupoEconomico`+`Contrato` numa migration só, dificulta
rollback seletivo).

---

## 5. Camada de Aplicação (CQRS)

### 5.1 Commands

| Command | Request | O que faz |
|---|---|---|
| `CriarContratoCommand` | `ClienteId`, `Pcr`, `Segmento`, `Tipo`, `SituacaoMes`, `Bandeira`, `RegistradoAle`, `InicioVigencia`, `FimVigencia`, `VolumeMensalContratado`, `MargemBase`, `Diretoria`, `RegionalVendas`, `PontoVenda`, `Consultor`, `Greenfield?` | Valida `ClienteId` existe (`IRepository<Cliente>.ExisteAsync`), valida `Pcr` único (`IContratoRepository.ExistePorPcrAsync`), instancia `new Contrato(...)`, `AdicionarAsync` |
| `RegistrarEventoContratoCommand` | `ContratoId`, `Tipo`, `Data`, `Descricao` | Carrega via `ObterComEventosAsync`, chama `contrato.RegistrarEvento(...)`, `AtualizarAsync` — cobre Renovação/Readequação/Cessão/Sucessão/Denúncia/Encerramento, não precisa de um Command por tipo de evento |
| `AtualizarGalonagemFaturadaCommand` | `ContratoId`, `NovaGalonagemFaturada` | Uso interno/futuro pela integração SAP/PCR (Fase 5) — criar o Command agora, mesmo sem endpoint público ainda, para o `Worker` (quando existir) já ter o que chamar sem precisar tocar em `Contrato` de novo |

Seguem exatamente o padrão de `CriarClienteCommandHandler`: `Result<T>`,
`DomainException` capturada e convertida em `Result.Failure(ex.Errors)`,
**sem** `IUnitOfWork` manual (decorator transacional cuida disso, DT-021).

### 5.2 Queries

`IContratoQueries` (implementação em `Data.SqlServer`, usa
`ApplicationDbContext` direto — **nunca** `IContratoRepository`, DT-018):

```csharp
public interface IContratoQueries : IQuery
{
    Task<ConsultarContratoResponse?> ConsultarContratoPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginationResponse<ListarContratosResponse>> ListarContratosAsync(
        ListarContratosRequest request, CancellationToken cancellationToken = default);
}
```

`ListarContratosRequest` cobre exatamente os filtros que a Listagem (F2 do
frontend, já implementada sobre mock) espera — ver
`frontend/src/app/contratos/ContratosPageClient.tsx`:

```csharp
public record ListarContratosRequest : PaginationRequest, ISortableRequest, IRequest<Result<PaginationResponse<ListarContratosResponse>>>
{
    public string? Busca { get; init; }                    // razão social, CNPJ, PCR, SAP, grupo — busca combinada
    public IReadOnlyList<Segmento>? Segmentos { get; init; }
    public IReadOnlyList<TipoContrato>? Tipos { get; init; }
    public IReadOnlyList<SituacaoPcr>? SituacoesPcr { get; init; }  // ver nota abaixo — filtra por valor CALCULADO
    public bool? EmDenuncia { get; init; }
    public string? Bandeira { get; init; }
    public IReadOnlyList<string>? Ufs { get; init; }
    public string? Diretoria { get; init; }
    public string? RegionalVendas { get; init; }
    public string? PontoVenda { get; init; }
    public string? Consultor { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}
```

**Filtrar por `SituacaoPcr` calculado, sem trazer tudo pra memória:**
`galPct`/`prazoExpirado` são expressões simples (`decimal`/comparação de
data) — dá pra escrever a mesma lógica de `Contrato.CalcularGalonagem`
**diretamente na projeção LINQ** (que o EF Core traduz pra SQL), em vez de
chamar o método de domínio (que não é traduzível). Extrair os dois num
método `static` puro compartilhado por domínio e query evita duplicar a
regra:

```csharp
// PortalAle.Domain.Contratos.GalonagemCalculo (static, sem estado — usável tanto no método de instância quanto em Expression<Func<...>> da query)
public static class GalonagemCalculo
{
    public static SituacaoPcr Calcular(decimal galonagemContratada, decimal galonagemFaturada, DateOnly fimVigencia, DateOnly referencia)
    {
        decimal saldo = Math.Max(0, galonagemContratada - galonagemFaturada);
        decimal percentual = galonagemContratada == 0 ? 0 : galonagemFaturada / galonagemContratada;
        bool cumpriu = percentual >= 1 || saldo <= 0;
        bool expirado = fimVigencia < referencia;

        return cumpriu ? SituacaoPcr.VencidoPorGalonagem : expirado ? SituacaoPcr.VencidoPorData : SituacaoPcr.Vigente;
    }
}
```

`Contrato.CalcularGalonagem` (domínio) chama esse método estático; a Query
monta a mesma expressão via `Select` — como a lógica é aritmética simples
(sem chamada de método complexo), o EF Core traduz para SQL sem problema.
Isso **não viola DT-018** (regra de negócio continua definida uma vez só, no
domínio) — só reaproveita a mesma fórmula em dois lugares por necessidade de
tradução SQL, prática usual em CQRS com read models otimizados.

`ListarContratosResponse` espelha `frontend/src/types/contrato.ts` →
`ContratoListItem` (campos já validados contra a tela real: `contratoId`,
`razao`, `cnpj`, `sapCliente`, `grupo`, `uf`, `segmento`, `tipo`,
`galPct`/`galPctStr` equivalente, `situacaoPcr`, `fimVigencia`,
`projVencGalonagem`, `juridicoAtivo` — este último **sempre `false`/omitido**
até a integração Elaw existir, não inventar dado).

### 5.3 Ordenação (`s_sortMap`, mesmo padrão de `ClienteQueries`)

Colunas ordenáveis (mesmas da tabela da Listagem, F2): `pcr` (na verdade
ordena por `NrPcr`/prefixo calculado — ordenar por `NrPcr` já basta,
protótipo confirma `contratoId` = mesma ordenação textual), `razao` (via
`Cliente.Nome`, precisa de `Join`/`.Include(c => c.Cliente)` na query),
`grupo`, `segmento`, `galPct` (expressão calculada, ver 5.2), `fimVigencia`,
`projVencGalonagem` (calculado).

---

## 6. Endpoints REST (`ContratosEndpoints.cs`, DT-006)

Mesmo padrão de `ClientesEndpoints.cs`, grupo `/api/v{version:apiVersion}/contratos`:

| Verbo | Rota | Handler | Retorno sucesso | Retorno erro |
|---|---|---|---|---|
| `GET` | `/` | `ListarContratos` (`[AsParameters] ListarContratosRequest`) | `200` `PaginationResponse<ListarContratosResponse>` | `400` |
| `GET` | `/{id:int}` | `ConsultarContrato` | `200` `ConsultarContratoResponse` | `404` |
| `POST` | `/` | `CriarContrato` | `201` + `Location` | `400` (`ToValidationProblem`) |
| `POST` | `/{id:int}/eventos` | `RegistrarEventoContrato` | `200`/`201` | `404`/`400` |

Não criar `PUT`/`DELETE` genéricos — não há caso de uso de "editar contrato
inteiro" ou "excluir contrato" no protótipo/contexto (contratos se encerram
via evento, não são deletados). Se surgir necessidade real depois, adicionar
então — não pré-construir CRUD completo sem caso de uso (linha do CLAUDE.md
raiz: "não adicione features além do que a tarefa exige").

---

## 7. Testes (DT-008)

**Bloqueio compartilhado com `PLANO-IMPLEMENTACAO.md` Fase 0.1:** hoje **não
existe nenhum projeto de teste na solução** (`GestaoContratoAle.sln` só tem
`Application`/`Data`/`Data.SqlServer`/`Domain`/`IoC`/`Api`). Antes de
`Contrato` ganhar testes, os projetos abaixo precisam existir — se a Fase 0.1
ainda não foi feita para `Cliente`, fazer junto (mesmo padrão serve para as
duas entidades, não duplicar esforço):

- `GestaoContratoAle.Domain.Tests` (MSTest + Moq + Bogus, **não** xUnit —
  DT-022 mostra exemplos em xUnit mas **DT-008 é quem manda no projeto: MSTest
  é obrigatório**. Seguir DT-008, os exemplos do DT-022 são só ilustrativos
  de outro projeto de origem)
- `GestaoContratoAle.Application.Tests`

Cobertura mínima (DT-008, 70%):

- **Domínio** (`ContratoTests`, `GalonagemCalculoTests`, `EventoContratoTests`):
  - `Criar_ComPcrVazio_DeveLancarDomainException`
  - `Criar_ComFimVigenciaAnteriorAoInicio_DeveLancarDomainException`
  - `Criar_ComVolumeMensalZeroOuNegativo_DeveLancarDomainException`
  - `Criar_ComDadosValidos_DeveRegistrarEventoNovoNegocioAutomaticamente`
  - `CalcularGalonagem_ComFaturamentoAbaixoDoContratadoEPrazoVigente_DeveRetornarVigente`
  - `CalcularGalonagem_ComFaturamentoIgualOuMaiorQueContratado_DeveRetornarVencidoPorGalonagemComBarraVerde` (nome do teste já documenta a regra de negócio da barra, seção 9 — mesmo cuidado tomado nos testes do frontend, ver `frontend/src/mocks/contratos.test.ts`)
  - `CalcularGalonagem_ComPrazoExpiradoESemCumprirGalonagem_DeveRetornarVencidoPorData`
  - `RegistrarEvento_EmContratoEncerrado_DeveLancarDomainException`
  - `RegistrarEvento_ComTipoEncerramento_DeveMarcarContratoComoEncerrado`
- **Aplicação** (`CriarContratoCommandHandlerTests`, `RegistrarEventoContratoCommandHandlerTests`):
  repositório/queries mockados via Moq, seguindo exatamente o exemplo AAA do
  DT-008 (`CriarProjetoAsync_ComDadosValidos_DeveCriarProjeto`).
- **Não testar** (decisão já registrada no DT-008, reaplicada aqui):
  `ContratoQueries` (QueryHandlers não são testados), `ContratoRepository`
  (repositórios não são testados), `ContratosEndpoints` (testes
  arquiteturais, responsabilidade do Arquiteto de Soluções conforme DT-008).

---

## 8. O que este plano **não** resolve — não assumir

Lista explícita (regra do `CLAUDE.md` raiz — itens "em aberto" não devem ser
implementados sem confirmação):

| Item | Onde fica registrado | Impacto se implementado sem confirmar |
|---|---|---|
| Origem real da hierarquia comercial (`Diretoria`/`RegionalVendas`/etc.) | Seção 2 do `PLANO-IMPLEMENTACAO-FRONTEND.md`, seção 14 do contexto | Se vier do SAP, os 4 campos textuais viram read-only/sincronizados — schema atual (texto livre) sobrevive sem migration, mas o Command de criação mudaria |
| `GuardaChuva` completo + "CNPJ pode ter mais de uma PCR ativa simultaneamente?" | `PLANO-IMPLEMENTACAO.md` 1.2 | Decide se `PapelGuardaChuva`/`CdGuardaChuva` (campos simples deste plano) evoluem para FK de um aggregate `GuardaChuva` de verdade |
| Mecanismo de atualização da bandeira ANP | Seção 14 do contexto | `DsBandeira` continua editável manualmente até lá |
| TIR Realizada/Projetada, Reappraise, gráficos de Performance | Seção 10 do contexto, Fase 6 | Não modelados nesta tabela — pertencem a E2/E3 quando desbloqueados |
| Campos/API do Elaw (aba Jurídico) | Skill `integracao-elaw`, Fase 5 | `Contrato` não tem e não deve ganhar colunas de ações/notificações jurídicas |
| Volume realmente faturado (fonte SAP/PCR) | Fase 5 | `MdGalonagemFaturada` fica em `0` até lá — `situacaoPcr` nunca será "Vencido por galonagem" na prática até a integração existir |

---

## 9. Ordem de execução

- [x] **F-A** — `GrupoEconomico` mínimo (entidade, migration, CQRS
      Criar/Consultar/Listar, endpoints) + migration complementar em
      `Cliente` (`IdGrupoEconomico`, `NrSap`, `SgUf`) — seção 1
- [x] **F-B** — Projetos de teste (`Domain.Tests`, `Application.Tests`)
      criados do zero (não existia nenhum `*.Tests.csproj` na solução) —
      seção 7
- [x] **F-C** — Domínio `Contrato`/`EventoContrato`/enums + testes unitários
      (17 testes) — seção 3, 7
- [x] **F-D** — `ContratoConfiguration`/`EventoContratoConfiguration` +
      migration `CriarTabelaContrato` — seção 4
- [x] **F-E** — `IContratoRepository`/`ContratoRepository` — seção 3.4
- [x] **F-F** — Commands (`CriarContrato`, `RegistrarEventoContrato`,
      `AtualizarGalonagemFaturada`) + testes (6 testes) — seção 5.1, 7
- [x] **F-G** — `IContratoQueries`/`ContratoQueries` com paginação, ordenação
      e todos os filtros da Listagem (F2 do frontend) — seção 5.2
- [x] **F-H** — `ContratosEndpoints` — seção 6
- [x] **F-I** — Trocar o mock do frontend (`useContratos()`,
      `frontend/src/hooks/useContratos.ts`) pelo `GET /api/v1/contratos` real
      — Fase F5 do `PLANO-IMPLEMENTACAO-FRONTEND.md`, feito em 2026-08-06
- [x] **F-J** (não estava no plano original) — Seed de desenvolvimento
      (`DesenvolvimentoSeeder`, roda automaticamente em `Development` após
      `Database.Migrate()`) portando os mesmos 14 contratos do mock do
      frontend para o banco real — CNPJs recalculados com dígito verificador
      válido (o mock usa CNPJs fictícios que não passam na validação de
      domínio de `Cliente`), demais campos mapeados 1:1. Ver seção 2 para o
      "de-para" campo a campo que também guiou o seed.

**F-A a F-H concluídos e validados em 2026-08-05** — `dotnet build`/`dotnet
test` (23 testes) verdes, e todos os endpoints exercitados de ponta a ponta
com `curl` contra um SQL Server local real (não só compilação): `POST`/`GET`
de `Cliente`/`GrupoEconomico`/`Contrato`, `POST .../eventos`, listagem
paginada com todos os filtros (segmento, situação da PCR calculada, busca,
hierarquia comercial) e as duas direções de ordenação.

**Bugs de infraestrutura compartilhada encontrados e corrigidos durante a
validação** (não eram do escopo original do plano, mas bloqueavam a
execução — ver memória de projeto para detalhe completo):
1. `GestaoContratoAle.sln` e os `<ProjectReference>` de todos os `.csproj`
   ainda apontavam para os caminhos antigos `PortalAle.*` (o rename para
   `GestaoContratoAle.*` do commit `254cce9` só renomeou pastas/arquivos,
   não atualizou essas referências) — a solução inteira não compilava antes
   desta sessão.
2. Enums de domínio não deserializavam do JSON (`System.Text.Json` sem
   `JsonStringEnumConverter` configurado) — corrigido em `Program.cs`,
   afeta todos os endpoints, não só Contrato.
3. `PaginationRequest.PageIndex`/`PageSize` (`int` não-anulável) fazia o
   Minimal API tratar como parâmetro obrigatório de query string — qualquer
   `GET` de listagem sem `?pageIndex=`/`?pageSize=` explícitos quebrava com
   500, **incluindo o `GET /api/v1/clientes` já existente**. Corrigido para
   `int?` em `IPaginationRequest`/`PaginationRequest`.
4. Filtros/ordenação por campos calculados (`SituacaoPcr`, `Segmento`) não
   traduzem para SQL quando aplicados **depois** do `Select` final — LINQ
   precisou ser reestruturado para filtrar/ordenar no estágio intermediário
   (antes da projeção para o record de resposta). Ver comentários em
   `ContratoQueries.cs`.
5. **CORS** — nenhuma API .NET libera origem cruzada por padrão; sem uma
   política explícita, o navegador bloqueia o `fetch` do frontend
   (`localhost:3000`) para a API (`localhost:5080`) com "Failed to fetch",
   mesmo com a API saudável e acessível via `curl`/Postman (CORS é uma
   restrição do browser, não do servidor) — só apareceu ao testar a
   integração de verdade no Chrome, não em nenhum teste automatizado. Fixo
   em `Program.cs` (`AddCors`/`UseCors`, origens configuráveis via
   `Cors:AllowedOrigins`, default `http://localhost:3000`).
6. **`openapi-fetch` + MSW**: o client (`createClient`) captura a referência
   de `fetch` no momento da criação (import time), antes do `beforeAll` dos
   testes rodar `server.listen()` — toda chamada furava o mock do MSW e ia
   para a rede de verdade, silenciosamente (sem erro, só resultado errado).
   Fix: passar `fetch: (...args) => globalThis.fetch(...args)` na criação do
   client (`src/lib/apiClient.ts`) para resolver `fetch` a cada chamada, não
   uma vez só.

**Critério de pronto do conjunto:** ✅ atingido e validado além do previsto
originalmente — além do backend, o frontend (`/contratos`) foi de fato
religado à API real e testado no Chrome contra dados seedados (`GET
/api/v1/contratos` paginado, filtros, busca com debounce, ordenação,
paginação) — não ficou só no `curl`. `dotnet test` (23) e `npm test` (80)
verdes.
