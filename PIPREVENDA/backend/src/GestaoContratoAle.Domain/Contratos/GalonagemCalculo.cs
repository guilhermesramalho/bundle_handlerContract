namespace PortalAle.Domain.Contratos;

/// <summary>
/// Regra de negócio de galonagem (contexto seção 9), extraída como função pura
/// estática para poder ser reaproveitada tanto pelo domínio (<see cref="Contrato.CalcularGalonagem"/>)
/// quanto pela camada de Query (que precisa da MESMA fórmula escrita como expressão LINQ
/// traduzível para SQL — chamar este método estático dentro de uma query EF Core não seria
/// traduzível, então a Query replica a fórmula inline; este tipo existe para o domínio e para
/// documentar/testar a fórmula uma única vez).
/// </summary>
public static class GalonagemCalculo
{
    public static SituacaoPcr CalcularSituacao(decimal galonagemContratada, decimal galonagemFaturada, DateOnly fimVigencia, DateOnly referencia)
    {
        decimal saldo = Math.Max(0, galonagemContratada - galonagemFaturada);
        decimal percentual = galonagemContratada == 0 ? 0 : galonagemFaturada / galonagemContratada;

        bool cumpriuGalonagem = percentual >= 1 || saldo <= 0;
        bool prazoExpirado = fimVigencia < referencia;

        return cumpriuGalonagem
            ? SituacaoPcr.VencidoPorGalonagem
            : prazoExpirado
                ? SituacaoPcr.VencidoPorData
                : SituacaoPcr.Vigente;
    }
}
