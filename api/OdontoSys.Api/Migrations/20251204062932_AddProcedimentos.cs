using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoSys.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProcedimentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Consultas",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Consultas",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Consultas",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Diagnostico",
                table: "Consultas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ObservacoesDaConsulta",
                table: "Consultas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Prescricao",
                table: "Consultas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Procedimentos",
                columns: table => new
                {
                    IdProcedimento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorPadrao = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procedimentos", x => x.IdProcedimento);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_IdDentista",
                table: "Consultas",
                column: "IdDentista");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_Dentistas_IdDentista",
                table: "Consultas",
                column: "IdDentista",
                principalTable: "Dentistas",
                principalColumn: "IdDentista",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_Dentistas_IdDentista",
                table: "Consultas");

            migrationBuilder.DropTable(
                name: "Procedimentos");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_IdDentista",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Diagnostico",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "ObservacoesDaConsulta",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Prescricao",
                table: "Consultas");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Consultas",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
