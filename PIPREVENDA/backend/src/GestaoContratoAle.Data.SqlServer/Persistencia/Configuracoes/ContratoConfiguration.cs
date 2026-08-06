using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortalAle.Domain.Contratos;

namespace PortalAle.Data.SqlServer.Persistencia.Configuracoes;

/// <summary>
/// Mapeamento EF Core de Contrato. Ver DT-003/DT-004 e PLANO-IMPLEMENTACAO-API-CONTRATO.md seção 4.
/// </summary>
public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("Contrato", t => t.HasComment("Base contratual consolidada (Rede/GRR/B2B)"));

        builder.HasKey(c => c.Id).HasName("PK_Contrato");

        builder.Property(c => c.Id)
            .HasColumnName("IdContrato")
            .HasColumnType("int")
            .ValueGeneratedOnAdd()
            .IsRequired()
            .HasComment("Identificador único do contrato");

        builder.Property(c => c.ClienteId)
            .HasColumnName("IdCliente")
            .HasColumnType("int")
            .IsRequired()
            .HasComment("Cliente (CNPJ) titular do contrato");

        builder.Property(c => c.Pcr)
            .HasColumnName("NrPcr")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("Número da PCR/PCF (sem prefixo — prefixo PCR/PCF é derivado do Segmento na apresentação)");

        builder.Property(c => c.Segmento)
            .HasColumnName("DsSegmento")
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("Segmento do contrato (Rede/Grr/Cofa/Cofd/Cofar/Outros/Spot)");

        builder.Property(c => c.Tipo)
            .HasColumnName("DsTipoContrato")
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired()
            .HasComment("Tipo do contrato (Pcvm/Imagem/Comodato)");

        builder.Property(c => c.SituacaoMes)
            .HasColumnName("DsSituacaoMes")
            .HasConversion<string>()
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired()
            .HasComment("Situação do contrato no mês (Ativo/Inativo), vinda do SICOF/PCR");

        builder.Property(c => c.Bandeira)
            .HasColumnName("DsBandeira")
            .HasColumnType("nvarchar(60)")
            .HasMaxLength(60)
            .IsRequired()
            .HasComment("Bandeira ANP — atualização hoje manual, mecanismo automático é item em aberto");

        builder.Property(c => c.RegistradoAle)
            .HasColumnName("InRegistradoAle")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador se o posto está registrado como bandeira ALE junto à ANP");

        builder.Property(c => c.DataRegistroAnp)
            .HasColumnName("DtRegistroAnp")
            .HasColumnType("date")
            .IsRequired(false)
            .HasComment("Data do último registro/atualização junto à ANP");

        builder.Property(c => c.InicioVigencia)
            .HasColumnName("DtInicioVigencia")
            .HasColumnType("date")
            .IsRequired()
            .HasComment("Data de início da vigência contratual");

        builder.Property(c => c.FimVigencia)
            .HasColumnName("DtFimVigencia")
            .HasColumnType("date")
            .IsRequired()
            .HasComment("Data de fim da vigência contratual");

        builder.Property(c => c.VolumeMensalContratado)
            .HasColumnName("MdVolumeMensalContratado")
            .HasColumnType("decimal(12,3)")
            .IsRequired()
            .HasComment("Volume mensal contratado (m³) — base para o cálculo de galonagem contratada");

        builder.Property(c => c.GalonagemFaturada)
            .HasColumnName("MdGalonagemFaturada")
            .HasColumnType("decimal(15,3)")
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Galonagem faturada acumulada — atualizada pela integração SAP/PCR (Fase 5); 0 até lá");

        builder.Property(c => c.MargemBase)
            .HasColumnName("PrMargemBase")
            .HasColumnType("decimal(5,2)")
            .IsRequired()
            .HasComment("Taxa de margem base do contrato");

        builder.Property(c => c.TirContratada)
            .HasColumnName("PrTirContratada")
            .HasColumnType("decimal(5,2)")
            .IsRequired(false)
            .HasComment("TIR Contratada — fixa na aprovação (única das 3 TIRs persistida aqui, ver seção 10 do contexto)");

        builder.Property(c => c.Greenfield)
            .HasColumnName("InGreenfield")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de posto Greenfield (novo, em curva de maturação)");

        builder.Property(c => c.Denuncia)
            .HasColumnName("InDenuncia")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de contrato em denúncia");

        builder.Property(c => c.Garantia)
            .HasColumnName("InGarantia")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de garantia contratual");

        builder.Property(c => c.Sublocado)
            .HasColumnName("InSublocado")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de imóvel sublocado de terceiro");

        builder.Property(c => c.Encerrado)
            .HasColumnName("InEncerrado")
            .HasColumnType("bit")
            .IsRequired()
            .HasDefaultValue(false)
            .HasComment("Indicador de contrato encerrado (mutado via Contrato.RegistrarEvento com TipoEventoContrato.Encerramento)");

        builder.Property(c => c.PapelGuardaChuva)
            .HasColumnName("DsPapelGuardaChuva")
            .HasConversion<string>()
            .HasColumnType("varchar(10)")
            .HasMaxLength(10)
            .IsRequired(false)
            .HasComment("Papel no guarda-chuva (Principal/Adicional) — versão embutida enquanto GuardaChuva completo (Fase 1.2) está bloqueado");

        builder.Property(c => c.CodigoGuardaChuva)
            .HasColumnName("CdGuardaChuva")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsRequired(false)
            .HasComment("Código de agrupamento do guarda-chuva");

        builder.Property(c => c.Diretoria)
            .HasColumnName("DsDiretoria")
            .HasColumnType("nvarchar(60)")
            .HasMaxLength(60)
            .IsRequired()
            .HasComment("Diretoria comercial — snapshot textual, origem (SAP vs. manual) é decisão em aberto");

        builder.Property(c => c.RegionalVendas)
            .HasColumnName("DsRegionalVendas")
            .HasColumnType("nvarchar(60)")
            .HasMaxLength(60)
            .IsRequired()
            .HasComment("Regional de vendas (GR) — snapshot textual");

        builder.Property(c => c.PontoVenda)
            .HasColumnName("DsPontoVenda")
            .HasColumnType("nvarchar(60)")
            .HasMaxLength(60)
            .IsRequired()
            .HasComment("Ponto de venda/revenda (RN) — snapshot textual");

        builder.Property(c => c.Consultor)
            .HasColumnName("NmConsultor")
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired()
            .HasComment("Consultor comercial responsável");

        builder.Property(c => c.Observacao)
            .HasColumnName("TxObservacao")
            .HasColumnType("nvarchar(max)")
            .IsRequired(false)
            .HasComment("Observações livres sobre o contrato");

        builder.Property(c => c.Clausula)
            .HasColumnName("TxClausula")
            .HasColumnType("nvarchar(max)")
            .IsRequired(false)
            .HasComment("Cláusulas específicas do contrato");

        builder.Property(c => c.CnpjSucedido)
            .HasColumnName("NrCnpjSucedido")
            .HasColumnType("varchar(14)")
            .HasMaxLength(14)
            .IsRequired(false)
            .HasComment("CNPJ do contrato/cliente predecessor, em caso de Sucessão");

        builder.Property(c => c.RazaoSucedido)
            .HasColumnName("NmRazaoSucedido")
            .HasColumnType("nvarchar(200)")
            .HasMaxLength(200)
            .IsRequired(false)
            .HasComment("Razão social do predecessor, em caso de Sucessão");

        builder.Property(c => c.DataCriacao)
            .HasColumnName("DhInclusao")
            .HasColumnType("datetimeoffset")
            .IsRequired()
            .HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .HasComment("Data e hora de criação do registro");

        builder.Property(c => c.DataAlteracao)
            .HasColumnName("DhAlteracao")
            .HasColumnType("datetimeoffset")
            .IsRequired(false)
            .HasComment("Data e hora da última alteração do registro");

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .HasConstraintName("FK_Contrato_Cliente")
            .OnDelete(DeleteBehavior.Restrict);

        // Eventos é exposta só como IReadOnlyCollection (encapsulamento — mutação só via
        // Contrato.RegistrarEvento); EF Core acessa o campo `_eventos` diretamente.
        builder.Metadata.FindNavigation(nameof(Contrato.Eventos))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(c => c.Eventos)
            .WithOne(e => e.Contrato)
            .HasForeignKey(e => e.ContratoId)
            .HasConstraintName("FK_HistoricoEventoContrato_Contrato")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ClienteId).HasDatabaseName("IX_Contrato_IdCliente");
        builder.HasIndex(c => c.Segmento).HasDatabaseName("IX_Contrato_DsSegmento");
        builder.HasIndex(c => c.FimVigencia).HasDatabaseName("IX_Contrato_DtFimVigencia");

        builder.HasIndex(c => c.Pcr)
            .HasDatabaseName("UQ_Contrato_NrPcr")
            .IsUnique();
    }
}
