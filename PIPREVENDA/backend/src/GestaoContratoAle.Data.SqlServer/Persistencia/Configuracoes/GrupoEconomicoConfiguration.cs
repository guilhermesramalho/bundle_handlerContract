using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalAle.Domain.GruposEconomicos;

namespace PortalAle.Data.SqlServer.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento EF Core de GrupoEconomico. Ver DT-003/DT-004.
/// </summary>
public class GrupoEconomicoConfiguration : IEntityTypeConfiguration<GrupoEconomico>
{
    public void Configure(EntityTypeBuilder<GrupoEconomico> builder)
    {
        builder.ToTable("GrupoEconomico", t => t.HasComment("Grupo econômico — conjunto de CNPJs do mesmo grupo controlador (cadastro manual, placeholder até integração SAP)"));

        builder.HasKey(grupo => grupo.Id)
            .HasName("PK_GrupoEconomico");

        builder.Property(grupo => grupo.Id)
            .HasColumnName("IdGrupoEconomico")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasComment("Identificador único do grupo econômico");

        builder.Property(grupo => grupo.Codigo)
            .HasColumnName("CdSap")
            .HasColumnType("varchar(3)")
            .HasMaxLength(3)
            .IsRequired()
            .HasComment("Código SAP do grupo econômico (3 dígitos)");

        builder.Property(grupo => grupo.Nome)
            .HasColumnName("NmGrupo")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Nome do grupo econômico");

        builder.Property(grupo => grupo.DataCriacao)
            .HasColumnName("DhInclusao")
            .HasColumnType("datetimeoffset")
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasComment("Data e hora de criação do registro");

        builder.Property(grupo => grupo.DataAlteracao)
            .HasColumnName("DhAlteracao")
            .HasColumnType("datetimeoffset")
            .IsRequired(false)
            .HasComment("Data e hora da última alteração do registro");

        builder.HasIndex(grupo => grupo.Codigo)
            .HasDatabaseName("UQ_GrupoEconomico_CdSap")
            .IsUnique();
    }
}
