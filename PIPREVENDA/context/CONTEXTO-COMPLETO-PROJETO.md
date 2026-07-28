# CONTEXTO DE PROJETO — Sistema de Gestão de Contratos — ALE Combustíveis
**Código:** PIPREVENDA-1680
**Consolidado em:** 27/07/2026, a partir de todos os artefatos e transcrições do projeto (pasta `documentos/` e `documentos_arq/`)
**Propósito deste documento:** contexto único e completo para uso como conhecimento de projeto no Claude — substitui a necessidade de ler todos os artefatos individualmente. Consolida a proposta comercial original (CONTEXT v3, 15/06), com todas as evoluções posteriores (feedbacks de protótipo de 23/06 a 03/07, aprofundamento de TIR/PIR de 14/07 e arquitetura técnica de 18/07 — as fontes mais recentes têm precedência sobre a v3 onde houver conflito).

---

## 1. Visão geral em uma página

A **ALE Combustíveis** é uma distribuidora de combustíveis que opera uma rede de postos de revenda (segmentos Rede/GRR) e atende clientes B2B de grande volume (segmentos COFA/COFD/COFAR). A ALE não é dona dos postos: fornece combustível, comodato de equipamentos e incentivos financeiros a revendedores sob sua bandeira.

Hoje a gestão de mais de **20 mil contratos ativos** é feita inteiramente em **planilhas Excel desconectadas** — uma para Rede, outra para B2B, outra para análise de performance ("reappraise"), outra para o book executivo. Atualização manual, feita por analistas (principalmente Igor Oliveira) nos primeiros dias úteis de cada mês, copiando e colando dados entre arquivos.

A migração da ALE para o **SAP** tornou isso urgente: o SAP cobre a parte financeira (faturamento, cadastro), mas não gerencia o **ciclo de vida do contrato** (múltiplas PCRs por revendedor, performance, histórico de eventos). Esse é exatamente o gap que este projeto resolve.

**A solução:** um sistema web (módulo satélite) que centraliza tudo, atualizado automaticamente via integração com SAP, com o novo sistema de PCR (em desenvolvimento por outro fornecedor), com o Elaw (jurídico) e com a ANP (bandeira do posto), publicando dados no Data Lake corporativo da ALE.

**Onde o projeto está agora (27/07/2026):** Fase 2 — Discovery/Prototipação. Protótipo funcional construído (Claude Design/Figma-like) e já apresentado em 4 rodadas de validação com stakeholders (23/06, 25/06, 29/06 interno, 03/07). Arquitetura técnica-alvo v2 documentada em 18/07 por Yuri Najar. Ainda **nenhuma linha de código de produto foi escrita**. Vários pontos de integração (PCR, SAP, Elaw, ANP) seguem bloqueados/pendentes de definição técnica externa.

---

## 2. Pessoas do projeto

### Time Performa_IT
| Papel | Nome |
|---|---|
| Responsável pelo projeto | Thiago Macedo |
| Product Manager | Rubens Fernandes |
| UX / Prototipação | André Amaral (e, a partir de 03/07, também **Vitor**, novo UX/PD compartilhando a função) |
| Arquiteto | Yuri Najar |
| Customer Success | Larissa Chaves |

### Time ALE Combustíveis
| Nome | Papel |
|---|---|
| **Suzana Yamada** | Gestora/coordenadora de contratos. Interlocutora principal, aprovadora operacional. |
| **Ana Caldas** | Gerente Executiva de Planejamento. Aprovadora executiva; consome dashboards, não opera o sistema. |
| **Igor Oliveira** | Analista de contratos; maior conhecimento operacional das planilhas atuais; usuário central do sistema futuro. |
| **Eros Ferreira** | Analista de performance; consome bases para análises estratégicas. |
| **Talita Campos** | Suporte operacional / acompanhamento do projeto junto ao fornecedor. |
| **Nicole** e **Ícaro** | Time de performance ALE; participaram das apresentações de protótipo (23/06 e 25/06). |
| **Lelio Lins**, **Daniel Sarnes** | TI ALE — referência técnica para SAP, SICOF, Data Lake. |
| **Natália Romão** | Responsável pelo módulo de PCR (outro fornecedor, projeto paralelo). Ponto focal crítico para integração, reappraise, TIR e PIR. |
| **Thaísa** | Time jurídico ALE / Elaw — apresentou o sistema Elaw em detalhe (07/07). |
| **Larissa (jurídica)** | Coordenadora jurídica ALE; aprova mudanças de status de ações no Elaw. |
| **Fernanda** | Jurídico ALE — pendente de especificar campos de garantia. |

---

## 3. O problema (detalhado)

- **Dados fragmentados**: planilhas separadas por segmento (Rede/B2B) e por finalidade (base, galonagem, reappraise, book), mantidas por pessoas diferentes, sem sincronismo entre si.
- **Atualização manual mensal**: rotina de vários dias, todo início de mês, sujeita a erro de copiar/colar.
- **Sem visão por produto**: hoje só existe volume total; nenhuma planilha quebra por Etanol/Gasolina/Diesel/GNV/Lubrificantes.
- **Gap criado pelo SAP**: SAP não gerencia ciclo de vida contratual (múltiplas PCRs, sucessões, performance, histórico).
- **Jurídico e ANP desconectados**: cruzamento manual com Elaw (ações, notificações) e com o site da ANP (bandeira do posto).
- **Escala do problema**: +20 mil contratos ativos — qualquer processo manual é um risco operacional real, já materializado em retrabalho e decisões com dados desatualizados.

---

## 4. Escopo — Entregáveis (E1–E6)

Abordagem de entrega incremental (sugerida, a confirmar formato final com cliente):
**Fase 1** E1 + E5(parcial) → **Fase 2** E2 + E5(complemento) → **Fase 3** E6 → **Fase 4** E3 + E4

| # | Entregável | O que faz |
|---|---|---|
| **E1** | Base Contratual Consolidada | Substitui planilhas Rede/B2B por base única, visão por CNPJ e por PCR/PCF, todos os segmentos, guarda-chuva, ciclo de vida completo (novo negócio, renovação, readequação, cessão/sucessão, denúncia, encerramento). Carga de legado como atividade de back-office (não é feature de usuário). |
| **E2** | Acompanhamento de Performance | Automatiza galonagem mensal, volume médio configurável, projeção de vencimento, visão por produto, Greenfield/curva de maturação, relatório mensal automatizado, filtro de vencimento. |
| **E3** | Reappraise (Análise de Performance) | Gráficos realizado vs. contratado (volume, margem, investimento, contribuição marginal), TIR contratada/realizada/projetada com racional auditável, status visual (OK/Recuperação/Crítico). |
| **E4** | Módulo Jurídico | Dados do Elaw: ações ativas por tipo (não só rescisão), notificações com grau e justificativa, garantia, sublocação, status de denúncia. |
| **E5** | Integrações SAP e Data Lake | Bidirecional com SAP (cadastro, segmento, faturamento, pagamento), publicação no Data Lake, mecanismo de bandeira ANP. |
| **E6** | Book de Contratos (Visão Executiva) | Dashboard de portfólio, funil de vencimentos, histórico de inaugurações/renovações, exportação (Excel/PDF). |

**Fora de escopo desta fase**: pagamento de multas/concessões, gestão de frota comercial, PIR completa (fase futura, sessão dedicada com Natália Romão pós-lançamento do PCR, previsto agosto/2026).

---

## 5. Personas

| Persona | Nome representativo | Papel | Dor principal |
|---|---|---|---|
| Gestora de Contratos | Suzana Yamada | Coordena Rede+B2B, decide renovações/ações comerciais | Consolidação manual antes de qualquer análise |
| Analista de Contratos | Igor Oliveira | Constrói/atualiza as bases mensalmente | Rotina manual mensal; histórico disperso |
| Analista de Performance | Eros Ferreira | Análises estratégicas de performance | Sem visão por produto; estudos manuais e demorados |
| Gestora Executiva | Ana Caldas | Consome dashboards/relatórios, não opera o sistema | Sem relatório automatizado de volume/margem |
| Suporte Operacional | Talita Campos | Acompanha rotinas e o projeto junto ao fornecedor | — |

> Não haverá controle de permissionamento por perfil na v1 — todos os usuários internos têm acesso equivalente. **Mas isso não significa "sem login"**: autenticação (Azure AD B2C) continua obrigatória. Consultores comerciais não terão acesso ao sistema nesta fase (ponto levantado por Vitor em 03/07, confirmado por Suzana) — possibilidade futura de o sistema alimentar/substituir o portal de consultores, sem impacto agora.

---

## 6. Jornadas

- **J1 — Consulta e Análise de um Contrato** (Igor, Suzana): busca por CNPJ/PCR/SAP, visualiza dados, histórico, galonagem, performance, jurídico.
- **J2 — Acompanhamento Mensal da Base** (Igor, Talita): substitui a rotina manual; base atualizada sozinha após fechamento SAP.
- **J3 — Identificação de Contratos por Vencer** (Suzana, Igor, Ana Caldas): filtro por prazo ou por galonagem.
- **J4 — Análise de Contratos com Eventos Jurídicos** (Suzana): filtro/consulta de dados do Elaw.
- **J5 — Visão Executiva do Portfólio** (Ana Caldas, Suzana): dashboard, funil de vencimentos, exportação.

---

## 7. Funcionalidades (F01–F21) — visão condensada

> Numeração original do CONTEXT v3. Regras completas (entradas/saídas/critérios de aceite) estão no artefato `[ARTEFATO] CONTEXT..._v3.md`; abaixo, o essencial + tudo que mudou depois via feedback de protótipo (seção 8).

| ID | Funcionalidade | Entregável | Regra-chave |
|---|---|---|---|
| F01 | Visualização de contratos por CNPJ/PCR-PCF | E1 | Sistema 100% consultivo — nenhum dado cadastrado manualmente; tudo vem de integrações |
| F02 | Guarda-chuva (principal + adicionais) | E1 | Ver regras de galonagem diferentes Rede vs. B2B na seção 9 |
| F03 | Eventos do ciclo de vida contratual | E1 | Tipos: novo negócio, renovação, readequação, cessão, sucessão, denúncia, encerramento — nenhum outro tipo mapeado |
| F04 | Filtros e busca avançada | E1 | Combináveis, sem "salvar filtro" na v1; ver hierarquia comercial atualizada (seção 8) |
| F05 | Galonagem por contrato | E2 | Saldo = Contratado − Faturado; % Cumprimento = Faturado/Contratado; atualização automática pós-fechamento SAP |
| F06 | Volume médio configurável | E2 | Até 12 meses, configurável pelo usuário |
| F07 | Projeção de vencimento por galonagem | E2 | Data projetada = Hoje + (Saldo ÷ Volume médio mensal); Greenfield usa meta da curva, não histórico |
| F08 | Situação de galonagem | E2 | Vigente / Vencido — ver desdobramento em "vencido por galonagem" vs. "vencido por data" (seção 9) |
| F09 | Visão por quebra de produto | E2 | Etanol, Gasolina C, Gasolina Aditivada, Diesel S10, Diesel S500, GNV, Lubrificantes, outros |
| F10 | Greenfield e curva de maturação | E2 | Sistema só exibe a curva (vem da PCR), não a calcula |
| F11 | Relatório mensal automatizado | E2 | Exportável em Excel e PDF; sem template fixo ainda |
| F12 | Filtro de proximidade de vencimento | E2 | Por prazo e/ou por galonagem, atalhos 1/3/6/12 meses + range customizado |
| F13 | Gráfico de desempenho (realizado x contratado) | E3 | Expandido para 4 gráficos — ver seção 8 |
| F14 | TIR realizada vs. projetada | E3 | Expandido significativamente — ver seção 10 |
| F15 | Informações jurídicas por contrato | E4 | Escopo ampliado de "só rescisão" para todas as ações — ver seção 11 |
| F16 | Filtro por eventos jurídicos em período | E4 | Considera ações + notificações |
| F17 | Bandeira ANP | E5 | Mecanismo de atualização (integração vs. manual) ainda em aberto |
| F18 | Dashboard executivo de portfólio | E6 | Números macro, com drill-down até contrato individual |
| F19 | Funil de vencimentos | E6 | Atalhos 1/3/6/12 meses + range customizado, por prazo e por galonagem |
| F20 | Histórico de inaugurações e renovações | E6 | Definição operacional de "inauguração" ainda pendente de confirmação com o cliente |
| F21 | Exportação de relatórios | E6 | Excel + PDF, disponível em toda lista/visão do sistema |

### Premissas transversais confirmadas
Dispositivos: desktop principal, responsivo tablet/mobile · Volume: +20.000 contratos (arquitetura deve prever paginação/índices desde o início) · Notificações: e-mail como canal principal, com abertura futura para Teams/WhatsApp · Log de auditoria: sim, sempre (quem/o quê/quando) · Sem permissionamento de perfil na v1 · Plataforma web.

---

## 8. Evolução do protótipo — feedbacks e decisões pós-CONTEXT v3

> As reuniões de apresentação do protótipo (23/06, 25/06, 29/06 interno, 03/07) geraram mudanças e novos itens não presentes na v3 original. Estas são as fontes **mais recentes** e têm precedência.

### Confirmado e já implementado no protótipo (validado sem pedido de mudança em 03/07)
- Coluna de grupo econômico na listagem + busca por texto (não é filtro dropdown — lista seria enorme sem contexto)
- Número SAP do cliente (10 dígitos, ex: 1000123456) exibido junto a CNPJ; busca funciona por CNPJ, nº SAP ou grupo econômico
- Coluna "Venc. projetado" (vencimento por galonagem, últimos 12 meses ou período disponível) coexistindo com o vencimento contratual
- Status "vencido" desdobrado em vencido por galonagem (positivo) e vencido por data (negativo) — ver seção 9
- Esquema de cor da barra de galonagem: verde =100%+ cumpriu · amarelo = em progresso · vermelho = vencido por data sem cumprimento
- Histórico de contratos encerrados acessível no detalhe (abre com tag "Encerrado", só leitura, sem tag de galonagem ativa, mas com aba de galonagem histórica)
- Seção de grupo econômico no detalhe, com código SAP do grupo (3 dígitos)
- Indicador de sublocação nos dados cadastrais (sim/não + prazo de vigência) — ~50 postos sublocados, fonte Elaw (dados legados podem estar incompletos)
- Campo de garantia (sim/não + botão "ver detalhes", ainda placeholder)
- Gráfico de investimento aprovado vs. realizado na aba de performance
- Contribuição marginal (meta vs. realizado) na aba de performance
- Aba PIR criada como placeholder ("a refinar — sessão específica com Natália Romão")
- Filtros do dashboard executivo espelhando a tela inicial

### Novo — regra confirmada em 03/07
**Vencimento projetado posterior ao vencimento contratual → pintar em vermelho.** Quando a projeção de galonagem for depois do vencimento contratual, isso indica risco (cliente não vai cumprir a galonagem no prazo) e deve compor os futuros alertas. *("Sempre que a data do vencimento projetado for maior do que o vencimento, a gente pinta em vermelho" — André, confirmado por Ana, 03/07.)*

### Novo — filtro de UF
UF (estado) é atributo geográfico independente da estrutura comercial (um GR pode atender várias UFs) — filtro próprio na barra de filtros gerais.

### Alterado — hierarquia comercial (origem: Lelio Lins, 03/07, confirmado por Suzana e André)
- **Remover:** Gerente Executivo, Regional (2º nível, redundante), Supervisor
- **Manter:** Diretor, Gerente Regional, Coordenador (quando existir na área)
- **Renomear:** "Representante Comercial" → "**Consultor Comercial**"
- Filtro em cascata com seleção múltipla via checkbox: nível superior restringe o inferior, mas qualquer nível é filtrável diretamente. Mesma limpeza aplicada na aba de carteira do dashboard executivo.

### Alterado — identificação de contrato
Campo único de identificação (não mais dois campos separados PCR/PCF) — PCR é para Rede, PCF para B2B, mesmo tipo de identificador, só muda o prefixo.

### Regra — listagem por PCR (mantida por ora)
Tabela organizada por PCR/PCF, não por CNPJ, **enquanto não se confirma se um CNPJ pode ter mais de uma PCR ativa simultaneamente**. Se puder, ambas devem aparecer. *(Ponto em aberto — ver seção 14.)*

### Aba de performance — 4 gráficos + TIR
Linha superior com 4 gráficos: **Volume mensal | Margem | Investimento (aprovado vs. realizado) | Contribuição marginal**. Bloco inferior separado: TIR (contratada/realizada/projetada). Margem no reappraise é sempre pelo mix total de produtos, nunca por produto individual.

### Tag de status do reappraise
Tag colorida no cabeçalho do contrato (não ícone de farol): **verde=OK** (performando como esperado) · **amarelo=Recuperação** (melhorando, ainda abaixo) · **vermelho=Crítico** (piorando ou muito abaixo). Regras de classificação **ainda pendentes** — Suzana precisa enviar a base do reappraise com critérios de score; implementado por ora como placeholder com os três estados visíveis.

### Novo (03/07) — sistema de gatilhos e alertas
Suzana pediu que o sistema identifique situações críticas e **dispare fluxos acionáveis** (não apenas informativos) para jurídico/comercial. Hoje o processo é manual: quando um contrato vence sem cumprir a galonagem, é preciso gerar estudo de galonagem e notificar via jurídico — se atrasa, a ALE perde força jurídica.

Exemplos de gatilhos citados: cliente saiu da bandeira ANP · contrato venceu sem cumprir galonagem · não cumprimento de galonagem mensal · prazo de notificação jurídica se aproximando.

Estrutura de mapeamento por gatilho: **Quando** (condição) → **Quem responde** (jurídico/consultor/GR) → **Canal** (e-mail/WhatsApp) → **Template** (texto pré-preenchido editável).

> ⚠️ **Não implementar no protótipo nem comprometer no cronograma** sem antes receber a lista de gatilhos do Igor Oliveira e o aval técnico de Yuri Najar sobre viabilidade e impacto no cronograma. Considerar como candidato a **F22** (não formalizado ainda no CONTEXT).

---

## 9. Regras de negócio críticas (não reabrir sem justificativa)

- **Guarda-chuva Rede**: cada CNPJ do grupo tem galonagem **individual**.
- **Guarda-chuva B2B** (COFA/COFD/COFAR): galonagem **global**, compartilhada por todos os CNPJs do grupo.
- **Vencido por galonagem** (cliente cumpriu 100%+ antes do prazo — vencimento *positivo*): chip vermelho, **barra de galonagem verde**.
- **Vencido por data** (prazo expirou sem cumprimento — vencimento *negativo*, requer ação comercial): chip vermelho, **barra de galonagem vermelha**. O chip nunca muda de cor entre os dois casos — é a barra que diferencia.
- **Tipos de contrato**: **PCVM** (volume mínimo obrigatório, permanece vigente até cumprir mesmo após o prazo) · **Imagem** (Rede, sem volume mínimo, vence na data — sendo descontinuado) · **Comodato** (equivalente B2B do Imagem, pouco usado).
- **Segmentos**: Rede (ativo) / GRR (rede em depuração) / COFA-COFD-COFAR (B2B ativo) / Outros (B2B em depuração) / Spot (sem contrato vigente).
- **Grupo econômico**: conjunto de CNPJs do mesmo grupo controlador — pode conter guarda-chuvas, contratos independentes, ou ambos. Identificado por código SAP de 3 dígitos.
- **Número SAP do cliente** (10 dígitos, ex.: 1000123456) identifica o **cliente**; **PCR/PCF** identifica o **contrato** — são coisas diferentes e ambas aparecem no sistema.
- **Projeção de galonagem**: padrão 12 meses; se houver menos histórico, usa o disponível (captura sazonalidade, ex. postos litorâneos). No detalhe do contrato o período é livremente configurável pelo usuário.

---

## 10. TIR, VPL, Payback e PIR — aprofundamento (fonte: reunião com Natália Romão, 10/07, documentado 14/07)

### Hierarquia das três análises de investimento
| | Reappraise (já existe) | TIR (em construção) | PIR (fase futura) |
|---|---|---|---|
| Variáveis | Volume + margem de contribuição (2) | Fluxo de caixa completo do contrato | Fluxo de caixa real vs. aprovado, incl. caso base, greenfield, varejo |
| Frequência | Automático, mensal | — | Periódica, deliberada |
| Resultado | Status OK/Recuperação/Crítico | TIR, VPL, comprometimento de margem, payback | Diagnóstico completo do investimento |
| Quando usar | Acompanhamento comercial do dia a dia | Decisão sobre retorno do investimento | Decisões de renovação/repactuação/encerramento |

**Limitação do reappraise**: não considera o rebate pago ao cliente — pode mascarar a margem real (ex: margem aparente R$200/m³ menos R$50 de rebate = R$150 real). Ponto a discutir com Ana Caldas antes de qualquer decisão de implementação.

### O que compõe o fluxo de caixa da TIR
- **Receitas**: volume × margem do período, receitas de varejo (loja de conveniência, AliExpress), mensalidade de conveniência
- **Investimentos/custos**: engenharia (obras), mútuo (retornável/fundo perdido/parcelado), rebate, custo de servir, taxa de juros do retornável, imposto de renda, CAPEX
- **Resultado**: **TIR (%)**, **VPL** (R$, valor presente), **comprometimento de margem aprovado**, **payback** — Natália confirmou que os quatro indicadores devem aparecer, não só a TIR.

### As três versões da TIR
- **TIR Contratada** — fixa, calculada na aprovação (plano original, nunca muda)
- **TIR Realizada** — com base no que já aconteceu (retrato atual)
- **TIR Projetada** — estimativa de fechamento se o ritmo atual continuar. ⚠️ **Não pode ser calculada com o que o sistema tem hoje** — requer fluxo de caixa completo com variáveis ainda não mapeadas (especialmente receitas de varejo e parâmetros históricos aprovados). Por ora, exibida no protótipo apenas como referência visual.

### Detalhes técnicos críticos
- Cada contrato usa os **parâmetros financeiros vigentes no momento da sua aprovação** (custo de servir, taxa de juros, WOC) — não os parâmetros atuais, senão a comparação fica distorcida.
- **Mudança de metodologia a partir de agosto/2026**: contratos aprovados até julho/2026 usam "replacement margin"; a partir de agosto/2026, "lucro bruto" (linha diferente do DRE, impacto significativo nos números). O sistema deve indicar claramente qual metodologia se aplica a cada contrato, com base na data de aprovação.

### Seção de racional do cálculo da TIR (nova exigência, 03/07)
Abaixo dos 3 cards de TIR (Contratada/Realizada/Projetada), adicionar seção colapsável/acessível via botão "Ver racional" (não exibida por padrão) para auditoria, com **3 colunas**: PCR Contratada · Realizado até o mês de referência · Negociação/atualização mais recente. Linhas por bloco:

| Bloco | Linha | Confirmação |
|---|---|---|
| Performance | Volume (m³) | ✅ Confirmado |
| Performance | Margem média do período (R$/L) | ✅ Confirmado |
| Performance | Margem de Contribuição (R$) | ✅ Confirmado |
| Investimentos | Investimentos — total | ✅ Confirmado |
| Investimentos | Engenharia | ✅ Confirmado |
| Investimentos | Mútuo | ✅ Confirmado (fundo perdido, retornável, parcelado) |
| Devolução financeira | Devolução Financeira | ⚠️ A confirmar — não citada nas transcrições, aparece no modelo da Suzana |
| Devolução financeira | Multa PCVM | ✅ Confirmado |
| Devolução financeira | Equipamentos | ⚠️ A confirmar — Suzana em dúvida se entra |
| Resultado | **TIR (%)** | ✅ Confirmado |

> Pendência: Suzana prometeu enviar dois modelos de estudo (Radix + manual) para validar as linhas ⚠️ antes da implementação.

### PIR — por que é mais complexa que a TIR
- **Caso base**: em renovações, meses restantes do contrato anterior já foram contabilizados nele — não podem duplicar na PIR da renovação. Cumprimento antecipado melhora o resultado; atraso piora.
- **Greenfield**: atraso na abertura do posto (ex: projetado 3 meses, real 12 meses) destrói o resultado financeiro — muito difícil de recuperar depois.
- **Varejo**: se a loja de conveniência nunca abriu, o problema não é o combustível — sem esses dados mapeados, a PIR fica incompleta.
- Status atual: aba placeholder no protótipo. Conteúdo será especificado com Natália Romão após lançamento da PCR (previsto agosto/2026). Decisão pendente: reappraise será unificado à PIR ou permanece separado (decisão com participação de Ana Caldas).

---

## 11. Módulo Jurídico / Elaw — detalhamento

> Nas transcrições o sistema aparece como "Elaw", "ELOL", "ILO" foneticamente — todos o mesmo sistema jurídico da ALE. Elaw é chamado internamente na ALE também de "Ilol"/"ELOL".

### Escopo ampliado (decisão de 23/06)
Escopo original só cobria ações de **rescisão de contrato PCVM**. Ampliado para trazer **todas as ações ativas do CNPJ**, independente do tipo — inclui recuperação de crédito, execução de garantia (real estate), ações de terceiros contra a ALE, etc.

### Estrutura de notificações no Elaw (fluxo completo, mapeado em 06–07/07)
1. **Grau 1** (tom brando — "vamos tentar resolver amigavelmente") → **Grau 2** (mais agressivo) → **Grau 3** (comunica que vai judicializar se não resolver)
2. Cada notificação tem prazo de resposta (via de regra 15 dias, mas pode ser customizado por solicitação)
3. Fluxo de envio: solicitação → escritório externo de advocacia (5 dias úteis para enviar) → confirma envio + anexa notificação (telegrama) → aguarda AR (aviso de recebimento, ~10 dias, controlado pelos Correios) → confirma AR → status muda para **Finalizado**
4. Se **grau 3** não resolvido em 15 dias → sistema migra automaticamente para **Contencioso**, criando um item em status **Pré-cadastro** (ainda não é ação judicial de fato — é o "checklist do avisamento")
5. Escritório jurídico analisa viabilidade do pré-cadastro — pode encerrar antes mesmo de virar ação (contrato muito antigo, sem viabilidade)
6. Se segue: inicia citação/protocolo → status muda para **Ativo** (ação judicial de fato)
7. Ao final: status muda para **Encerrado** (por resolução via acordo, decisão judicial, ou renovação da PCR que engloba o contrato)

### Status possíveis no Elaw
**Ativo · Encerrado · Pré-cadastro · Baixa provisória** (processo pausado, com justificativa e aprovação da coordenadora jurídica) — **Suspenso** e **Removido** existem no sistema mas **não são usados hoje** (recomendação da Thaísa/jurídico: o sistema de gestão de contratos deve ler e refletir qualquer status que existir no Elaw, mesmo os não usados atualmente, para não perder informação se um dia passarem a ser usados).

### "Sub-área" = tipo de ação
Campo usado para classificar a natureza da ação (a ALE está em processo de dividir o Elaw formalmente por sub-áreas): **Realinhamento Rede** (ações de rescisão) · **Real Estate** (execução de garantia imobiliária/BDV) · **Recuperação de Crédito** (cobrança de títulos) · **Alienação**.

### Cada contrato/PCR tem múltiplos processos possíveis
Uma PCR pode ter vários contratos vinculados (ex.: PCVM, multa, compra/venda de equipamentos) — cada um pode ter seu próprio processo/ID no Elaw. **Todo contrato tem um ID no Elaw desde 2020** (mesmo sem nenhuma ocorrência jurídica) — contratos anteriores a 2020 podem não estar cadastrados, exigindo checagem manual.

### Campos identificados como disponíveis via Elaw
- **Ações**: ID do processo, tipo/subtipo de ação, status, fase (aguardando julgamento, execução, recursal etc.), resumo do caso, valor da causa, classificação (baixa por perda estratégica ou normal), motivo/observação de encerramento, custas judiciais (extraível), honorários (variável, não 100% extraível)
- **Notificações**: quantidade por CNPJ, data da última, grau (1/2/3), justificativa (hoje campo de texto livre — ALE já solicitou ao Elaw padronizar como lista suspensa; **os 5 tipos padrão de justificativa ainda não foram levantados** — pendência)
- **Garantias**: existência (sim/não), tipo — reais (hipoteca, alienação) e pessoais (fiança, com indicação de fiadores). Fiadores hoje concentrados no SICOF, não no Elaw — vai exigir associação/migração.
- **Sublocação**: indicação e prazo de vigência — "certamente" no Elaw, mas dados legados podem estar desatualizados (checar com time patrimonial)
- **Denúncia contratual**: deve migrar do campo atual da planilha para o campo jurídico/Elaw no sistema novo

### Recomendação de UX levantada (07/07, ainda não decidida no protótipo)
Em vez de replicar todos os campos do Elaw, considerar um **botão/link de redirecionamento para o Elaw** a partir da tela de detalhe jurídico — trazendo apenas um "resumo" no sistema de gestão de contratos. Ressalva: nem todo usuário do sistema de gestão de contratos tem acesso ao Elaw — nesse caso, a jornada deveria indicar "fale com quem tem acesso" (ex: Fernanda) em vez de redirecionar diretamente.

### Bloqueio de integração
A disponibilidade técnica da API do Elaw ainda depende de reunião com o time técnico do Elaw/documentação de API (solicitada, aguardando retorno — acompanhamento por Thiago Macedo). **Nenhuma estrutura de dados deve ser assumida como definitiva** até essa confirmação (RT-05 no registro de riscos).

---

## 12. Arquitetura técnica (v2, Yuri Najar, 18/07/2026)

> Status do projeto: nenhuma linha de código de produto escrita ainda — este é o desenho-alvo proposto.

### Backend — .NET 10, Clean Architecture (referência: solução `PortalAle`)
`Domain` (entidades puras: Contrato, Aditivo, Guarda-chuva, Grupo Econômico, Reappraise) → `Application` (CQRS commands/queries, portas `ISapIntegrationPort`/`IElawIntegrationPort`/`IContratoRepository`) → `Data`/`Data.SqlServer` (EF Core) → `Data.SapStaging` (isola staging do ADF do domínio) → `IoC` → `Api` (Controllers, versionamento, OpenAPI) → `Worker` (serviço hospedado separado, consumidor/publicador assíncrono, deploy independente da API).

**Staging → domínio (fluxo SAP de entrada)**: opção recomendada — pipeline ADF (ou trigger na staging) publica evento no Azure Service Bus; `Worker` consome via `ServiceBusProcessor` nativo (concorrência controlada, retry, dead-letter) e processa via `Application`/`Domain`. Opção B (trigger no banco + `Channel<T>` em memória) só se a ALE restringir o padrão — nesse caso exige persistência do estado "pendente" no banco e mecanismo de "claim" de linha entre réplicas do `Worker` (senão risco de perda de dados em restart e processamento duplicado com múltiplas réplicas).

### Frontend — React/Next.js
App Router por entregável (`/contratos`, `/performance`, `/reappraise`, `/juridico`, `/book`); módulos espelhando E1–E6; tipos TypeScript gerados a partir do OpenAPI da API .NET (evita drift). CSR nas telas de dados (paginação/streaming), SSR só no shell/autenticação — dado o volume de +20 mil contratos, SSR de tabelas grandes pioraria TTFB sem ganho real. React Query para data-fetching.

### Integrações
| Integração | Padrão | Observação |
|---|---|---|
| SAP entrada | ADF → staging → evento no Azure Service Bus → `Worker` | Aplicação nova, nenhum cenário de contratos reaproveita pipeline existente (RT-02) |
| SAP saída | `Worker`/`Api` → Azure Function (HTTP) → REST SAP | Síncrono, recomendado Polly (retry/circuit breaker) |
| PCR (novo sistema) | Assíncrono | Protocolo a definir com o fornecedor (RT-01) |
| Elaw | Síncrono ou assíncrono, a definir | Bloqueado até retorno sobre campos disponíveis via API |
| ANP | A definir | Pode não haver API pública/estruturada |
| Data Lake | Assíncrono | Formato a definir com TI ALE |
| SICOF | Batch único (não recorrente) | Carga histórica isolada, janela fechando com migração SAP (RT-03) |

Mensageria: **Azure Service Bus** (ALE já opera em outras frentes), abstraído via biblioteca (ex.: MassTransit) na camada `Application`/`Worker`. Padrão outbox/inbox + idempotência obrigatórios em tudo que passa pelo broker.

### Infraestrutura e operação
- **Auth**: Azure AD B2C (tenant já existente na ALE) — login obrigatório mesmo sem permissionamento de perfil na v1.
- **Infra**: Kubernetes + Helm (um chart por componente: api/worker/web), cluster em Azure (assumindo AKS, a confirmar). Service Bus/ADF/Function como PaaS externos ao cluster. Segredos via Key Vault/External Secrets, nunca em `values.yaml`.
- **CI/CD**: Azure DevOps, pipeline multi-stage (build → test → package → push ACR → deploy Helm por ambiente), variable groups por ambiente vinculados a Key Vault, gate manual antes de produção.
- **Branching**: Gitflow simplificado (`main`/`develop`/`release/*`/`hotfix/*`), versionamento semântico.

### Requisitos não-funcionais críticos
- **+20 mil contratos** → paginação obrigatória desde E1, índices desde o modelo de dados inicial, considerar CQRS/read models dedicados para o Book Executivo (E6)
- **Log de auditoria** desde E1 (interceptor EF Core ou tabela dedicada) — não é "adicionar depois"
- **Observabilidade ponta a ponta**: OpenTelemetry no Next.js e no .NET, trace correlacionado frontend → API → worker → broker → integrações
- **Dados sensíveis** (Elaw, contratuais): HTTPS obrigatório em todas as camadas, segredos fora do código, mascaramento em logs

### Decisões técnicas ainda em aberto (Seção 13 da arquitetura)
Tier do Azure Service Bus e namespace · necessidade real da Opção B (`Channel<T>`) · política de login B2C (sign-in only já existe?) · claims do token B2C · protocolo real da API do PCR (Natália) · protocolo real da API do Elaw (Thiago) · existência de API da ANP · convenção de nomenclatura de repositórios.

---

## 13. Registro de riscos

### Técnicos
| ID | Risco | Prob. | Impacto |
|---|---|---|---|
| RT-01 | Sistema de PCR (outro fornecedor) sem API/contrato de integração definido a tempo | Alta | Alto |
| RT-02 | Integrações SAP construídas do zero, documentação pode não existir | Alta | Alto |
| RT-03 | Acesso ao SICOF encerra após migração SAP — janela limitada para carga de legado | Alta | Médio |
| RT-04 | Extração do PCR 700 é lenta (~3h) e pode não ser reproduzível no novo ambiente | Média | Médio |
| RT-05 | Elaw pode não ter API disponível | Média | Baixo |
| RT-06 | Margem unitária pode não estar disponível na PCR | Média | Baixo |
| RT-07 | +20 mil contratos → risco de performance/carregamento | Alta | Médio |

### Prazo
RP-01 (pressão do cronograma da PCR, referência julho/2026, é referência não restrição) · RP-02 (disponibilidade da TI ALE limitada por outros projetos SAP) · RP-03 (carga de legado mais complexa que o esperado).

### Integração/Dependência
RI-01 (mudanças no PCR impactam Fase 1) · RI-02 (sincronismo com PCR durante migração SICOF) · RI-03 (guarda-chuva B2B pode ser modelado diferente no novo PCR).

### Negócio/Premissas
RN-01 (pagamento de concessão pode ser pedido como MVP mesmo fora de escopo) · RN-02 (alta variabilidade das regras contratuais pode alongar especificação do E2) · RN-03 (sem budget definido, risco de rejeição por valor) · RN-04 (dependência crítica de Igor Oliveira).

---

## 14. Pontos em aberto — nada deve ser assumido sem confirmação

| Item | Quem desbloqueia | Impacto |
|---|---|---|
| Base do reappraise (Excel com critérios de score) | Suzana Yamada | Regras do status OK/Recuperação/Crítico |
| 5 tipos de justificativa das notificações Elaw | André/Suzana | Tabela de notificações |
| Campos disponíveis na API do Elaw | Thiago Macedo (reunião em andamento) | Toda a aba jurídica |
| CNPJ pode ter mais de uma PCR ativa simultaneamente? | Cliente | Define se listagem migra de PCR para CNPJ |
| Formatos dos relatórios de diretoria e Glencore | Ana Caldas | Dashboard executivo |
| Sessão de reappraise e de PIR com Natália Romão | Suzana (agendamento) | Especificação da aba reappraise e da aba PIR |
| Regras de classificação do status do reappraise | Suzana Yamada | Lógica da tag de status |
| Definição operacional de "inauguração" | Cliente | F20 |
| Mecanismo de atualização da bandeira ANP | A definir | E5 |
| Rastreamento de investimento original em repactuações/cessões | Yuri + Natália | Cálculo correto da TIR |
| Modelos de estudo de TIR (Radix + manual) | Suzana Yamada | Validar linhas ⚠️ do racional de TIR |
| Lista de gatilhos/situações de notificação e variáveis | Igor Oliveira | Sistema de alertas (candidato F22) |
| Templates/roteiros de comunicação (e-mail/WhatsApp) dos alertas | Vitor + Igor | Sistema de alertas |
| Avaliação técnica de viabilidade dos alertas | Yuri Najar | Sistema de alertas |
| Especificação dos campos de garantia (ver detalhes) | Fernanda (jurídico ALE) | Módulo jurídico |

---

## 15. Glossário essencial

**PCR** — Proposta de Concessão de Revenda (Rede). **PCF** — equivalente para B2B. **PCR 700** — extração/relatório de PCRs via SICOF, hoje usado como fonte de dados. **PIR** — Revisão de Investimento Pós-Realização (fase futura). **Galonagem** — volume de combustível contratado/comprado. **Reappraise** — análise de performance (volume + margem), resulta em status OK/Recuperação/Crítico. **Contribuição marginal** — volume × margem unitária (R$), o "dinheiro real" gerado. **TIR** — Taxa Interna de Retorno do investimento. **VPL** — Valor Presente Líquido. **Contrato Guarda-Chuva** — CNPJ principal + adicionais vinculados. **Greenfield** — posto novo, meta progressiva (curva de maturação). **Grupo econômico** — CNPJs do mesmo grupo controlador (código SAP 3 dígitos). **Sublocação** — posto cujo imóvel pertence a terceiro (não a ALE nem ao revendedor). **Denúncia** — encerramento formal antecipado do contrato. **Destrato** — encerramento amigável ou litigioso. **Depuração** — remoção de cliente da gestão comercial ativa (vira GRR ou "Outros"). **Concessão/Fundo perdido/Rebate** — modalidades de incentivo financeiro (concessão está fora do escopo desta fase). **SAP** — ERP para o qual a ALE migra (fatura, cadastro; não gerencia ciclo de vida contratual). **SICOF** — sistema legado sendo substituído, acesso fechará após migração. **Elaw/ELOL/ILO** — sistema jurídico da ALE. **ANP** — órgão regulador da bandeira dos postos. **Data Lake** — repositório central de dados da ALE (já em operação). **Módulo satélite** — sistema independente conectado a um sistema central (aqui, ao SAP).

---

## 16. Como usar este contexto ao trabalhar no projeto

- **Fonte de verdade**: onde há conflito entre a v3 do CONTEXT (15/06) e os feedbacks/reuniões posteriores (23/06 em diante), **os documentos mais recentes prevalecem** — já refletido neste consolidado.
- **Itens marcados como "em aberto" (seção 14) não devem ser assumidos nem implementados** sem confirmação do cliente ou do PM.
- **Decisões da seção 9 (regras de negócio críticas) e seção 8 (confirmado/implementado) não devem ser reabertas** sem justificativa nova.
- **Sistema de alertas/gatilhos** (seção 8, final) é uma funcionalidade **candidata**, ainda sem aval técnico nem lista de gatilhos — não tratar como escopo fechado.
- Nenhuma integração (SAP, PCR, Elaw, ANP) tem contrato técnico definitivo hoje — tratar ausência/instabilidade dessas integrações como cenário normal do início do projeto, não exceção.
- Volume de +20 mil contratos deve influenciar qualquer decisão de modelagem, paginação ou UX de listagem desde o primeiro desenho.

---

*Documento consolidado a partir de: CONTEXT v3 (15/06), RESUMO (02/07), HANDOFF (02/07), GLOSSÁRIO (02/07), FEEDBACKS COMPILADOS v2 (06/07, incl. reunião 03/07), MAPEAMENTO ELAW (a partir de transcrições até 25/06), EXPLICACAO_TIR_PIR (14/07), transcrições brutas de 06/07 e 07/07 sobre Elaw, e ARQUITETURA v2 (18/07).*
*Gerado para uso como contexto de projeto no Claude — PIPREVENDA-1680.*
