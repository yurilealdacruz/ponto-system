using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ponto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoBatidaAjuste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoBatida",
                table: "SolicitacoesAjuste",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoBatida",
                table: "SolicitacoesAjuste");
        }
    }
}
