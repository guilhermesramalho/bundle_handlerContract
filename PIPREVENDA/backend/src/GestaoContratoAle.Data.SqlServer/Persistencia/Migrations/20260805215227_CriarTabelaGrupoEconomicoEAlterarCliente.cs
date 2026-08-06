using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalAle.Data.SqlServer.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaGrupoEconomicoEAlterarCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdGrupoEconomico",
                schema: "dbo",
                table: "Cliente",
                type: "int",
                nullable: true,
                comment: "Grupo econômico ao qual o cliente pertence (opcional)");

            migrationBuilder.AddColumn<string>(
                name: "NrSap",
                schema: "dbo",
                table: "Cliente",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                comment: "Número SAP do cliente (10 dígitos) — identifica o cliente (contexto seção 9)");

            migrationBuilder.AddColumn<string>(
                name: "SgUf",
                schema: "dbo",
                table: "Cliente",
                type: "char(2)",
                maxLength: 2,
                nullable: true,
                comment: "UF do CNPJ/ponto de venda");

            migrationBuilder.CreateTable(
                name: "GrupoEconomico",
                schema: "dbo",
                columns: table => new
                {
                    IdGrupoEconomico = table.Column<int>(type: "int", nullable: false, comment: "Identificador único do grupo econômico")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CdSap = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, comment: "Código SAP do grupo econômico (3 dígitos)"),
                    NmGrupo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Nome do grupo econômico"),
                    DhInclusao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()", comment: "Data e hora de criação do registro"),
                    DhAlteracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Data e hora da última alteração do registro")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoEconomico", x => x.IdGrupoEconomico);
                },
                comment: "Grupo econômico — conjunto de CNPJs do mesmo grupo controlador (cadastro manual, placeholder até integração SAP)");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_IdGrupoEconomico",
                schema: "dbo",
                table: "Cliente",
                column: "IdGrupoEconomico");

            migrationBuilder.CreateIndex(
                name: "UQ_GrupoEconomico_CdSap",
                schema: "dbo",
                table: "GrupoEconomico",
                column: "CdSap",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_GrupoEconomico",
                schema: "dbo",
                table: "Cliente",
                column: "IdGrupoEconomico",
                principalSchema: "dbo",
                principalTable: "GrupoEconomico",
                principalColumn: "IdGrupoEconomico",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_GrupoEconomico",
                schema: "dbo",
                table: "Cliente");

            migrationBuilder.DropTable(
                name: "GrupoEconomico",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_IdGrupoEconomico",
                schema: "dbo",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "IdGrupoEconomico",
                schema: "dbo",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "NrSap",
                schema: "dbo",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "SgUf",
                schema: "dbo",
                table: "Cliente");
        }
    }
}
