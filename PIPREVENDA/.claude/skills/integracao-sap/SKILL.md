---
name: integracao-sap
description: Padrão de integração com o SAP (entrada e saída) do PIPREVENDA-1680 — pipeline ADF, staging, Azure Service Bus, Worker consumidor, Azure Function de saída. Use sempre que a tarefa envolver leitura ou escrita de dados vindos/para o SAP, staging de dados, ou o Worker de processamento assíncrono.
---

# Integração — SAP (PIPREVENDA-1680)

## ⚠️ Status: sem contrato técnico definitivo

Nenhum ponto de integração com o SAP está fechado hoje (ver
`context/CONTEXTO-COMPLETO-PROJETO.md`, seção 14 "em aberto" e RT-02 no
registro de riscos). **Não assuma nome de campo, schema ou payload** — trate
a ausência/instabilidade como cenário normal do início do projeto.

## Padrão arquitetural definido (fonte: contexto, seção 12)

### Entrada (SAP → sistema)
```
SAP → ADF (pipeline) → tabela de staging → evento no Azure Service Bus → Worker (ServiceBusProcessor) → Application/Domain
```
- Opção recomendada: staging publica evento no Service Bus; `Worker` consome
  com concorrência controlada, retry e dead-letter nativos.
- Opção B (trigger no banco + `Channel<T>` em memória) **só se a ALE
  restringir o padrão A** — exige persistência de estado "pendente" e
  mecanismo de "claim" de linha entre réplicas do Worker (risco de perda de
  dado em restart / duplicação com múltiplas réplicas se não implementado).
- `Data.SapStaging` isola o staging do domínio — não misturar staging com
  entidades de `Domain`.

### Saída (sistema → SAP)
```
Worker/Api → Azure Function (HTTP) → REST SAP
```
- Síncrono, com Polly (retry/circuit breaker) recomendado.

## Regras para o agente

- Aplicar a skill `padroes-tecnicos-backend-dotnet` → DT-011 (Anti-Corruption
  Layer) para qualquer adaptador que fale com o SAP — o domínio nunca deve
  depender de DTO/schema do SAP diretamente.
- Mensageria: **Azure Service Bus**, abstraído via biblioteca (ex. MassTransit)
  na camada `Application`/`Worker`.
- **Padrão outbox/inbox + idempotência são obrigatórios** em tudo que passa
  pelo broker — não implementar consumidor sem isso.
- Se a tarefa exigir um campo/contrato ainda não confirmado, parar e
  sinalizar a lacuna em vez de inventar a estrutura.
