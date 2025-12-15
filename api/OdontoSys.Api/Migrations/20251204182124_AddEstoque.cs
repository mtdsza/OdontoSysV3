using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoSys.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estoque",
                columns: table => new
                {
                    IdItemEstoque = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    EstoqueMin = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.IdItemEstoque);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MovimentacoesGeraisEstoque",
                columns: table => new
                {
                    IdMovimentacaoGeral = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Quantidade = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Justificativa = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdItemEstoque = table.Column<long>(type: "bigint", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesGeraisEstoque", x => x.IdMovimentacaoGeral);
                    table.ForeignKey(
                        name: "FK_MovimentacoesGeraisEstoque_Estoque_IdItemEstoque",
                        column: x => x.IdItemEstoque,
                        principalTable: "Estoque",
                        principalColumn: "IdItemEstoque",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsoMateriaisConsulta",
                columns: table => new
                {
                    IdUsoMaterial = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Quantidade = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    IdConsulta = table.Column<long>(type: "bigint", nullable: false),
                    IdItemEstoque = table.Column<long>(type: "bigint", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsoMateriaisConsulta", x => x.IdUsoMaterial);
                    table.ForeignKey(
                        name: "FK_UsoMateriaisConsulta_Consultas_IdConsulta",
                        column: x => x.IdConsulta,
                        principalTable: "Consultas",
                        principalColumn: "IdConsulta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsoMateriaisConsulta_Estoque_IdItemEstoque",
                        column: x => x.IdItemEstoque,
                        principalTable: "Estoque",
                        principalColumn: "IdItemEstoque",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesGeraisEstoque_IdItemEstoque",
                table: "MovimentacoesGeraisEstoque",
                column: "IdItemEstoque");

            migrationBuilder.CreateIndex(
                name: "IX_UsoMateriaisConsulta_IdConsulta",
                table: "UsoMateriaisConsulta",
                column: "IdConsulta");

            migrationBuilder.CreateIndex(
                name: "IX_UsoMateriaisConsulta_IdItemEstoque",
                table: "UsoMateriaisConsulta",
                column: "IdItemEstoque");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimentacoesGeraisEstoque");

            migrationBuilder.DropTable(
                name: "UsoMateriaisConsulta");

            migrationBuilder.DropTable(
                name: "Estoque");
        }
    }
}
