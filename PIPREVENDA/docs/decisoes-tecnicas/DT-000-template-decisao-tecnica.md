# DT-000: Template de Decisão Técnica (Arquitetural Decision Record)

**Objetivo**: Este template serve como referência para documentação de **decisões técnicas arquiteturais**.  
_Cada DT deve ser salvo em um arquivo separado na pasta `/docs/decisoes-tecnicas/` com numeração sequencial._

Segue abaixo a estrutura do template:

---

# Título: _DT-XXX: <Título da decisão em até 1 linha>_

> **Metadados do Documento**  
> **Componente:** `Backend`  
> **Tipo:** Decisão Técnica
>
> **Propósito:** _[Resuma em 1 linha o objetivo principal desta decisão]_
>
> **Quando usar:** _[Descreva em 1-2 linhas quando este documento deve ser consultado]_
>
> **Palavras-chave:** `keyword1` `keyword2` `keyword3` `keyword4`

### Contexto

_Explique o problema, motivação ou necessidade que levou à decisão._  
Exemplo: _A aplicação precisa autenticar usuários de forma centralizada utilizando o IdP corporativo._

### Decisão

_Explique claramente a decisão tomada._  
Exemplo: _Usar OIDC Authorization Code + PKCE com tokens JWT assinados pelo IdP._

### Alternativas Consideradas

_Liste outras opções avaliadas e porque foram descartadas._  
Exemplo:

1. _Usar SAML — rejeitado: legado e maior complexidade._
2. _Criar autenticação própria — rejeitado: inseguro e difícil de manter._

### Consequências

_Liste impactos positivos e negativos da decisão._  
Exemplo:

- (+) Integração padronizada com sistemas corporativos.
- (–) Necessidade de lidar com rotação de chaves.

### Implementação ()

_Oriente como a decisão será aplicada._  
Exemplo: _Configurar `AddAuthentication().AddJwtBearer(...)` no backend._

### Verificação de Conformidade

_Liste itens verificáveis que confirmam a adoção correta desta decisão no projeto._  
_Este checklist serve para auditar se o padrão está sendo seguido._

Exemplo de checklist:

- [ ] _Todos os endpoints de API usam autenticação JWT configurada_
- [ ] _Configuração de validação de token está presente em `appsettings.json`_
- [ ] _Logs incluem informações de autenticação (sem expor tokens)_
- [ ] _Testes de integração cobrem cenários de autenticação_
- [ ] _Documentação Swagger reflete os requisitos de autenticação_

---
