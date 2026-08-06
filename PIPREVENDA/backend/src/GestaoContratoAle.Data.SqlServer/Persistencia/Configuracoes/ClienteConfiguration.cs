using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalAle.Domain.Clientes;

namespace PortalAle.Data.SqlServer.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento EF Core de Cliente. Nomenclatura de tabela/colunas/constraints
/// conforme DT-003 (PascalCase, código de classe por coluna, sem mnemônico de
/// tabela — exceto na coluna de Id).
/// </summary>
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Cliente", t => t.HasComment("Cadastro de clientes (pessoa jurídica)"));

        builder.HasKey(cliente => cliente.Id)
            .HasName("PK_Cliente");

        builder.Property(cliente => cliente.Id)
            .HasColumnName("IdCliente")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasComment("Identificador único do cliente");

        builder.Property(cliente => cliente.Nome)
            .HasColumnName("NmRazaoSocial")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Razão social do cliente");

        builder.Property(cliente => cliente.Cnpj)
            .HasColumnName("NrCnpj")
            .HasColumnType("varchar(14)")
            .HasMaxLength(14)
            .IsRequired()
            .HasComment("CNPJ do cliente (somente dígitos)");

        builder.Property(cliente => cliente.Endereco)
            .HasColumnName("DsEndereco")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsRequired()
            .HasComment("Endereço do cliente");

        builder.Property(cliente => cliente.Ativo)
            .HasColumnName("InAtivo")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Indicador se o cliente está ativo (default: 1)");

        builder.Property(cliente => cliente.DataCriacao)
            .HasColumnName("DhInclusao")
            .HasColumnType("datetimeoffset")
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasComment("Data e hora de criação do registro");

        builder.Property(cliente => cliente.DataAlteracao)
            .HasColumnName("DhAlteracao")
            .HasColumnType("datetimeoffset")
            .IsRequired(false)
            .HasComment("Data e hora da última alteração do registro");

        builder.Property(cliente => cliente.DataExclusao)
            .HasColumnName("DhExclusao")
            .HasColumnType("datetimeoffset")
            .IsRequired(false)
            .HasComment("Data e hora da exclusão lógica do registro (soft delete)");

        builder.Property(cliente => cliente.NrSap)
            .HasColumnName("NrSap")
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false)
            .HasComment("Número SAP do cliente (10 dígitos) — identifica o cliente (contexto seção 9)");

        builder.Property(cliente => cliente.Uf)
            .HasColumnName("SgUf")
            .HasColumnType("char(2)")
            .HasMaxLength(2)
            .IsRequired(false)
            .HasComment("UF do CNPJ/ponto de venda");

        builder.Property(cliente => cliente.GrupoEconomicoId)
            .HasColumnName("IdGrupoEconomico")
            .HasColumnType("int")
            .IsRequired(false)
            .HasComment("Grupo econômico ao qual o cliente pertence (opcional)");

        builder.HasOne(cliente => cliente.GrupoEconomico)
            .WithMany()
            .HasForeignKey(cliente => cliente.GrupoEconomicoId)
            .HasConstraintName("FK_Cliente_GrupoEconomico")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cliente => cliente.Cnpj)
            .HasDatabaseName("UQ_Cliente_NrCnpj")
            .IsUnique();

        builder.HasIndex(cliente => cliente.GrupoEconomicoId)
            .HasDatabaseName("IX_Cliente_IdGrupoEconomico");

        // Soft delete: registros excluídos logicamente não aparecem nas consultas por padrão.
        builder.HasQueryFilter(cliente => cliente.DataExclusao == null);
    }
}
