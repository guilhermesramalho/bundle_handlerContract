namespace PortalAle.Domain.Contratos;

/// <summary>Segmentos confirmados no negócio (contexto seção 9). "Posa" aparece no protótipo mas não é um segmento oficial — deliberadamente não incluído.</summary>
public enum Segmento { Rede, Grr, Cofa, Cofd, Cofar, Outros, Spot }

public enum TipoContrato { Pcvm, Imagem, Comodato }

public enum SituacaoMes { Ativo, Inativo }

public enum PapelGuardaChuva { Principal, Adicional }

/// <summary>Únicos 7 tipos válidos de evento no ciclo de vida do contrato — contexto seção 9.</summary>
public enum TipoEventoContrato { NovoNegocio, Renovacao, Readequacao, Cessao, Sucessao, Denuncia, Encerramento }

/// <summary>Resultado do cálculo de galonagem — nunca persistido, sempre recalculado contra uma data de referência (ver GalonagemCalculo).</summary>
public enum SituacaoPcr { Vigente, VencidoPorGalonagem, VencidoPorData }
