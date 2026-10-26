using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CP5.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Times",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Jogo = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Pais = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Ranking = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Times", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jogadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nickname = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    Funcao = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    Idade = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TimeId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogadores_Times_TimeId",
                        column: x => x.TimeId,
                        principalTable: "Times",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfisCompetitivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    KDA = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    WinRate = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    HorasJogadas = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    JogadorId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisCompetitivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfisCompetitivos_Jogadores_JogadorId",
                        column: x => x.JogadorId,
                        principalTable: "Jogadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_TimeId",
                table: "Jogadores",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfisCompetitivos_JogadorId",
                table: "PerfisCompetitivos",
                column: "JogadorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PerfisCompetitivos");

            migrationBuilder.DropTable(
                name: "Jogadores");

            migrationBuilder.DropTable(
                name: "Times");
        }
    }
}
