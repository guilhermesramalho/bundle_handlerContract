using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalAle.Data.SqlServer.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contrato",
                schema: "dbo",
                columns: table => new
                {
                    IdContrato = table.Column<int>(type: "int", nullable: false, comment: "Identificador único do contrato")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCliente = table.Column<int>(type: "int", nullable: false, comment: "Cliente (CNPJ) titular do contrato"),
                    NrPcr = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "Número da PCR/PCF (sem prefixo — prefixo PCR/PCF é derivado do Segmento na apresentação)"),
                    DsSegmento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "Segmento do contrato (Rede/Grr/Cofa/Cofd/Cofar/Outros/Spot)"),
                    DsTipoContrato = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "Tipo do contrato (Pcvm/Imagem/Comodato)"),
                    DsSituacaoMes = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, comment: "Situação do contrato no mês (Ativo/Inativo), vinda do SICOF/PCR"),
                    DsBandeira = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false, comment: "Bandeira ANP — atualização hoje manual, mecanismo automático é item em aberto"),
                    InRegistradoAle = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador se o posto está registrado como bandeira ALE junto à ANP"),
                    DtRegistroAnp = table.Column<DateOnly>(type: "date", nullable: true, comment: "Data do último registro/atualização junto à ANP"),
                    DtInicioVigencia = table.Column<DateOnly>(type: "date", nullable: false, comment: "Data de início da vigência contratual"),
                    DtFimVigencia = table.Column<DateOnly>(type: "date", nullable: false, comment: "Data de fim da vigência contratual"),
                    MdVolumeMensalContratado = table.Column<decimal>(type: "decimal(12,3)", nullable: false, comment: "Volume mensal contratado (m³) — base para o cálculo de galonagem contratada"),
                    MdGalonagemFaturada = table.Column<decimal>(type: "decimal(15,3)", nullable: false, defaultValue: 0m, comment: "Galonagem faturada acumulada — atualizada pela integração SAP/PCR (Fase 5); 0 até lá"),
                    PrMargemBase = table.Column<decimal>(type: "decimal(5,2)", nullable: false, comment: "Taxa de margem base do contrato"),
                    PrTirContratada = table.Column<decimal>(type: "decimal(5,2)", nullable: true, comment: "TIR Contratada — fixa na aprovação (única das 3 TIRs persistida aqui, ver seção 10 do contexto)"),
                    InGreenfield = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de posto Greenfield (novo, em curva de maturação)"),
                    InDenuncia = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de contrato em denúncia"),
                    InGarantia = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de garantia contratual"),
                    InSublocado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de imóvel sublocado de terceiro"),
                    InEncerrado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicador de contrato encerrado (mutado via Contrato.RegistrarEvento com TipoEventoContrato.Encerramento)"),
                    DsPapelGuardaChuva = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, comment: "Papel no guarda-chuva (Principal/Adicional) — versão embutida enquanto GuardaChuva completo (Fase 1.2) está bloqueado"),
                    CdGuardaChuva = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, comment: "Código de agrupamento do guarda-chuva"),
                    DsDiretoria = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false, comment: "Diretoria comercial — snapshot textual, origem (SAP vs. manual) é decisão em aberto"),
                    DsRegionalVendas = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false, comment: "Regional de vendas (GR) — snapshot textual"),
                    DsPontoVenda = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false, comment: "Ponto de venda/revenda (RN) — snapshot textual"),
                    NmConsultor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Consultor comercial responsável"),
                    TxObservacao = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Observações livres sobre o contrato"),
                    TxClausula = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Cláusulas específicas do contrato"),
                    NrCnpjSucedido = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true, comment: "CNPJ do contrato/cliente predecessor, em caso de Sucessão"),
                    NmRazaoSucedido = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Razão social do predecessor, em caso de Sucessão"),
                    DhInclusao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()", comment: "Data e hora de criação do registro"),
                    DhAlteracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Data e hora da última alteração do registro")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contrato", x => x.IdContrato);
                    table.ForeignKey(
                        name: "FK_Contrato_Cliente",
                        column: x => x.IdCliente,
                        principalSchema: "dbo",
                        principalTable: "Cliente",
                        principalColumn: "IdCliente",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Base contratual consolidada (Rede/GRR/B2B)");

            migrationBuilder.CreateTable(
                name: "HistoricoEventoContrato",
                schema: "dbo",
                columns: table => new
                {
                    IdHistoricoEventoContrato = table.Column<int>(type: "int", nullable: false, comment: "Identificador único do evento")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdContrato = table.Column<int>(type: "int", nullable: false, comment: "Contrato ao qual o evento pertence"),
                    DsTipoEvento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "Tipo do evento (NovoNegocio/Renovacao/Readequacao/Cessao/Sucessao/Denuncia/Encerramento)"),
                    DtEvento = table.Column<DateOnly>(type: "date", nullable: false, comment: "Data em que o evento ocorreu"),
                    TxDescricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Descrição do evento")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoEventoContrato", x => x.IdHistoricoEventoContrato);
                    table.ForeignKey(
                        name: "FK_HistoricoEventoContrato_Contrato",
                        column: x => x.IdContrato,
                        principalSchema: "dbo",
                        principalTable: "Contrato",
                        principalColumn: "IdContrato",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Histórico de eventos do ciclo de vida do contrato");

            migrationBuilder.CreateIndex(
                name: "IX_Contrato_DsSegmento",
                schema: "dbo",
                table: "Contrato",
                column: "DsSegmento");

            migrationBuilder.CreateIndex(
                name: "IX_Contrato_DtFimVigencia",
                schema: "dbo",
                table: "Contrato",
                column: "DtFimVigencia");

            migrationBuilder.CreateIndex(
                name: "IX_Contrato_IdCliente",
                schema: "dbo",
                table: "Contrato",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "UQ_Contrato_NrPcr",
                schema: "dbo",
                table: "Contrato",
                column: "NrPcr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoEventoContrato_IdContrato",
                schema: "dbo",
                table: "HistoricoEventoContrato",
                column: "IdContrato");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoEventoContrato",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Contrato",
                schema: "dbo");
        }
    }
}
