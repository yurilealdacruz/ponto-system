using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ponto.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitacoesAjuste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacoesAjuste",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FuncionarioId = table.Column<int>(type: "integer", nullable: false),
                    RegistroPontoId = table.Column<int>(type: "integer", nullable: true),
                    DataHoraSugerida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Justificativa = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataSolicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacoesAjuste", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAjuste_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolicitacoesAjuste_RegistrosPonto_RegistroPontoId",
                        column: x => x.RegistroPontoId,
                        principalTable: "RegistrosPonto",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAjuste_FuncionarioId",
                table: "SolicitacoesAjuste",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacoesAjuste_RegistroPontoId",
                table: "SolicitacoesAjuste",
                column: "RegistroPontoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolicitacoesAjuste");
        }
    }
}
