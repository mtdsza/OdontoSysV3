using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoSys.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataContratacao",
                table: "Funcionarios",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalarioBase",
                table: "Funcionarios",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Funcionarios",
                type: "varchar(13)",
                maxLength: 13,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataContratacao",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "SalarioBase",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Funcionarios");
        }
    }
}
