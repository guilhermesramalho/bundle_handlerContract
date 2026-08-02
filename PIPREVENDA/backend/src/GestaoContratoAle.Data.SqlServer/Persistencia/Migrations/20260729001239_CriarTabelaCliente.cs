using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortalAle.Data.SqlServer.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Cliente",
                schema: "dbo",
                columns: table => new
                {
                    IdCliente = table.Column<int>(type: "int", nullable: false, comment: "Identificador único do cliente")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NmRazaoSocial = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Razão social do cliente"),
                    NrCnpj = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false, comment: "CNPJ do cliente (somente dígitos)"),
                    DsEndereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Endereço do cliente"),
                    InAtivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Indicador se o cliente está ativo (default: 1)"),
                    DhInclusao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()", comment: "Data e hora de criação do registro"),
                    DhAlteracao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Data e hora da última alteração do registro"),
                    DhExclusao = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Data e hora da exclusão lógica do registro (soft delete)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.IdCliente);
                },
                comment: "Cadastro de clientes (pessoa jurídica)");

            migrationBuilder.CreateIndex(
                name: "UQ_Cliente_NrCnpj",
                schema: "dbo",
                table: "Cliente",
                column: "NrCnpj",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cliente",
                schema: "dbo");
        }
    }
}
