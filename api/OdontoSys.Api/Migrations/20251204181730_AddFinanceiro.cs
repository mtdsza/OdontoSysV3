using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoSys.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParcelasAReceber",
                columns: table => new
                {
                    IdParcela = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DataVencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IdOrcamento = table.Column<long>(type: "bigint", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelasAReceber", x => x.IdParcela);
                    table.ForeignKey(
                        name: "FK_ParcelasAReceber_Orcamentos_IdOrcamento",
                        column: x => x.IdOrcamento,
                        principalTable: "Orcamentos",
                        principalColumn: "IdOrcamento",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MovimentacoesFinanceiras",
                columns: table => new
                {
                    IdMovimentacao = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    IdParcelaPaga = table.Column<long>(type: "bigint", nullable: true),
                    DataMovimentacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesFinanceiras", x => x.IdMovimentacao);
                    table.ForeignKey(
                        name: "FK_MovimentacoesFinanceiras_ParcelasAReceber_IdParcelaPaga",
                        column: x => x.IdParcelaPaga,
                        principalTable: "ParcelasAReceber",
                        principalColumn: "IdParcela");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesFinanceiras_IdParcelaPaga",
                table: "MovimentacoesFinanceiras",
                column: "IdParcelaPaga");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelasAReceber_IdOrcamento",
                table: "ParcelasAReceber",
                column: "IdOrcamento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacoesFinanceiras");

            migrationBuilder.DropTable(
                name: "ParcelasAReceber");
        }
    }
}
