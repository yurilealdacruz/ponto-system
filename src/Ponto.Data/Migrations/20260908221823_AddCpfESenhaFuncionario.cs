using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ponto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCpfESenhaFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Funcionarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "Funcionarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Senha",
                table: "Funcionarios");
        }
    }
}
