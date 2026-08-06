using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalAle.Domain.Contratos;

namespace PortalAle.Data.SqlServer.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento EF Core de EventoContrato — entidade filha de Contrato, sem repositório
/// próprio (DT-018). Ver DT-003/DT-004.
/// </summary>
public class EventoContratoConfiguration : IEntityTypeConfiguration<EventoContrato>
{
    public void Configure(EntityTypeBuilder<EventoContrato> builder)
    {
        builder.ToTable("HistoricoEventoContrato", t => t.HasComment("Histórico de eventos do ciclo de vida do contrato"));

        builder.HasKey(e => e.Id).HasName("PK_HistoricoEventoContrato");

        builder.Property(e => e.Id)
            .HasColumnName("IdHistoricoEventoContrato")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasComment("Identificador único do evento");

        builder.Property(e => e.ContratoId)
            .HasColumnName("IdContrato")
            .HasColumnType("int")
            .IsRequired()
            .HasComment("Contrato ao qual o evento pertence");

        builder.Property(e => e.Tipo)
            .HasColumnName("DsTipoEvento")
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("Tipo do evento (NovoNegocio/Renovacao/Readequacao/Cessao/Sucessao/Denuncia/Encerramento)");

        builder.Property(e => e.Data)
            .HasColumnName("DtEvento")
            .HasColumnType("date")
            .IsRequired()
            .HasComment("Data em que o evento ocorreu");

        builder.Property(e => e.Descricao)
            .HasColumnName("TxDescricao")
            .HasColumnType("nvarchar(500)")
            .HasMaxLength(500)
            .IsRequired()
            .HasComment("Descrição do evento");

        builder.HasIndex(e => e.ContratoId).HasDatabaseName("IX_HistoricoEventoContrato_IdContrato");
    }
}
