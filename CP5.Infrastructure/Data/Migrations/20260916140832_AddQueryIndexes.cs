using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CP5.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Times_Jogo",
                table: "Times",
                column: "Jogo");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_Funcao",
                table: "Jogadores",
                column: "Funcao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Times_Jogo",
                table: "Times");

            migrationBuilder.DropIndex(
                name: "IX_Jogadores_Funcao",
                table: "Jogadores");
        }
    }
}
