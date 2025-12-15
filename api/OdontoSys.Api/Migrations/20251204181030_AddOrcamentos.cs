using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoSys.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrcamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orcamentos",
                columns: table => new
                {
                    IdOrcamento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DataEmissao = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    IdPaciente = table.Column<long>(type: "bigint", nullable: false),
                    IdDentista = table.Column<long>(type: "bigint", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Aprovado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orcamentos", x => x.IdOrcamento);
                    table.ForeignKey(
                        name: "FK_Orcamentos_Dentistas_IdDentista",
                        column: x => x.IdDentista,
                        principalTable: "Dentistas",
                        principalColumn: "IdDentista",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orcamentos_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrcamentoItens",
                columns: table => new
                {
                    IdOrcamentoItem = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ValorUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    IdOrcamento = table.Column<long>(type: "bigint", nullable: false),
                    IdProcedimento = table.Column<long>(type: "bigint", nullable: false),
                    OrcamentoIdOrcamento = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrcamentoItens", x => x.IdOrcamentoItem);
                    table.ForeignKey(
                        name: "FK_OrcamentoItens_Orcamentos_OrcamentoIdOrcamento",
                        column: x => x.OrcamentoIdOrcamento,
                        principalTable: "Orcamentos",
                        principalColumn: "IdOrcamento");
                    table.ForeignKey(
                        name: "FK_OrcamentoItens_Procedimentos_IdProcedimento",
                        column: x => x.IdProcedimento,
                        principalTable: "Procedimentos",
                        principalColumn: "IdProcedimento",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItens_IdProcedimento",
                table: "OrcamentoItens",
                column: "IdProcedimento");

            migrationBuilder.CreateIndex(
                name: "IX_OrcamentoItens_OrcamentoIdOrcamento",
                table: "OrcamentoItens",
                column: "OrcamentoIdOrcamento");

            migrationBuilder.CreateIndex(
                name: "IX_Orcamentos_IdDentista",
                table: "Orcamentos",
                column: "IdDentista");

            migrationBuilder.CreateIndex(
                name: "IX_Orcamentos_IdPaciente",
                table: "Orcamentos",
                column: "IdPaciente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrcamentoItens");

            migrationBuilder.DropTable(
                name: "Orcamentos");
        }
    }
}
