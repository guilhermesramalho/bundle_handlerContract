---
name: regras-negocio-contratos
description: Regras de negócio do domínio de Gestão de Contratos da ALE Combustíveis (PCR, PCF, Guarda-chuva, Grupo Econômico, Reappraise/TIR/PIR, ciclo de vida do contrato, segmentos Rede/GRR/COFA/COFD/COFAR). Use esta skill SEMPRE que a tarefa envolver modelagem de domínio, validação de regra de negócio, nomenclatura de entidade ou qualquer dúvida sobre "como o negócio da ALE funciona" — antes de assumir comportamento não documentado.
---

# Regras de negócio — Gestão de Contratos ALE (PIPREVENDA-1680)

## Fonte de verdade

Este projeto **não** duplica as regras de negócio aqui. A fonte única e
autoritativa é:

- `context/CONTEXTO-COMPLETO-PROJETO.md` — documento consolidado. Ler:
  - **Seção 9** — regras de negócio críticas (não reabrir sem justificativa nova)
  - **Seção 8** — o que já está confirmado/implementado
  - **Seção 14** — itens em aberto (**não implementar/assumir** sem confirmação do PM/cliente)
  - **Seção 15** — glossário (PCR, PCF, Guarda-chuva, Depuração, Denúncia, Destrato, Concessão, Fundo Perdido, Rebate, SICOF, Elaw, ANP, Data Lake)
  - **Seção 16** — como usar o contexto (regra de precedência entre versões de documento)
- `context/integracao-elaw.html` — mapeamento de campos jurídicos (ver skill `integracao-elaw`)

## Regras de ouro para o agente

1. **Nunca invente estrutura de dados ou regra de negócio** para algo listado
   como "em aberto" (seção 14) — sinalize a lacuna na resposta/PR em vez de
   preencher com suposição.
2. Onde houver conflito entre versões de documento, **o mais recente prevalece**
   (regra já definida na seção 16 do contexto).
3. **Volume**: mais de 20 mil contratos ativos — qualquer entidade/listagem
   nova deve nascer com paginação e índice, nunca "adicionar depois" (também
   reforçado no DT técnico de arquitetura, seção de requisitos não-funcionais).
4. Nenhuma integração (SAP, PCR, Elaw, ANP) tem contrato técnico fechado hoje
   — trate ausência/instabilidade dessas integrações como cenário normal, não
   exceção, e não hardcode payloads não confirmados.
5. Não há permissionamento por perfil na v1 — todos os usuários têm acesso
   equivalente; personas orientam desenho de tela, não controle de acesso.

## Quando aprofundar

- Dúvida sobre um termo do domínio → seção 15 (glossário) antes de perguntar ao usuário.
- Dúvida sobre cálculo financeiro (TIR/VPL/Reappraise/PIR) → seção 11 do contexto.
- Dúvida sobre o que aparece na aba Jurídico → skill `integracao-elaw`.
