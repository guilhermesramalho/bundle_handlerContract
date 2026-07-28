---
name: mensageria-service-bus
description: Padrão de mensageria assíncrona (Azure Service Bus) do PIPREVENDA-1680 — outbox/inbox, idempotência, Worker, uso com PCR/Data Lake. Use sempre que a tarefa envolver publicar/consumir eventos, criar um novo consumidor no Worker, ou qualquer integração assíncrona (PCR, Data Lake).
---

# Mensageria — Azure Service Bus (PIPREVENDA-1680)

## Padrão definido (fonte: contexto do projeto, seção 12)

- Broker: **Azure Service Bus** (ALE já opera com ele em outras frentes).
- Abstração via biblioteca (ex.: **MassTransit**) na camada
  `Application`/`Worker` — não acoplar handlers diretamente ao SDK do Service Bus.
- **Padrão outbox/inbox + idempotência são obrigatórios** em qualquer
  publicador/consumidor — sem exceção, mesmo em protótipo.
- `Worker` é um serviço hospedado **separado** da API, com deploy
  independente (Helm chart próprio).

## Integrações que usam esse padrão

| Integração | Direção | Observação |
|---|---|---|
| SAP (entrada) | staging → Service Bus → Worker | ver skill `integracao-sap` |
| PCR (novo sistema) | assíncrono | protocolo ainda a definir com o fornecedor (RT-01) — não hardcode contrato |
| Data Lake | assíncrono (publicação) | formato a definir com TI ALE |

## Regras para o agente

- Antes de criar um novo consumidor/publicador, verificar se já existe
  abstração equivalente em `Application`/`Worker` — não duplicar.
- Nunca implementar consumo "at-least-once" sem idempotência (chave de
  deduplicação) — reprocessamento de evento é esperado.
- Se o protocolo/payload de uma integração (ex. PCR) ainda não estiver
  confirmado, sinalizar a lacuna em vez de definir um contrato definitivo.
