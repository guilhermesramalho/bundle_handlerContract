---
name: integracao-elaw
description: Padrão de integração com o Elaw (sistema jurídico da ALE) do PIPREVENDA-1680 — aba Jurídico, campos mapeados, DTOs sugeridos, regras de acesso. Use sempre que a tarefa envolver a aba Jurídico, notificações, processos/contencioso, ou qualquer dado que venha do Elaw/ILO/Willow.
---

# Integração — Elaw / Jurídico (PIPREVENDA-1680)

## ⚠️ Status: bloqueado — documentação técnica da API não recebida

A disponibilidade técnica da API do Elaw depende de retorno do time
técnico do Elaw (acompanhamento por Thiago Macedo — RT-05 no registro de
riscos). **Nenhuma estrutura de dado deve ser assumida como definitiva.**

## Escopo confirmado (fonte: `context/integracao-elaw.html`)

- A intenção validada com o time jurídico é mostrar uma **prévia/resumo** do
  que existe no Elaw — **não recriar a tela do Elaw**.
- Container da aba se chama **"Ações ativas no Elaw"** (não "ações de
  rescisão" — escopo foi ampliado em 06/07 para: rescisão, recuperação de
  crédito, reintegração de posse, real estate).
- Contrato de integração deve priorizar campos de **identificação, status e
  resumo** — não o detalhamento completo de tramitação/anexos/comunicações
  judiciais.
- Ideia em aberto (não decidida): deep-link do produto direto para o registro
  no Elaw — depende de mapear quem tem acesso direto ao Elaw.
- Se o usuário do sistema de gestão de contratos não tiver acesso ao Elaw, a
  jornada deve indicar "fale com quem tem acesso" (ex: Fernanda/Jurídico ALE)
  em vez de redirecionar diretamente.
- Confirmado: **não existe hoje** prognóstico de êxito para processos em que
  a ALE é ré — não implementar essa funcionalidade via API automática.
- Vinculação entre notificação grau 3 e abertura de pré-cadastro é
  **manual** hoje (planilha do time jurídico) — não modelar como transição
  automática rastreável.
- CNPJ é a chave de busca, mas há risco de inconsistência em grupos
  econômicos com múltiplas razões sociais sob uma "parte principal".

## Regras para o agente

- Todos os campos jurídicos mapeados e DTOs sugeridos estão em
  `context/integracao-elaw.html` — consultar antes de propor um contrato novo.
- Aplicar `padroes-tecnicos-backend-dotnet` → DT-011 (Anti-Corruption Layer)
  para o adaptador do Elaw.
- Síncrono vs assíncrono: **ainda a definir** — não decidir unilateralmente,
  sinalizar como dependência.
