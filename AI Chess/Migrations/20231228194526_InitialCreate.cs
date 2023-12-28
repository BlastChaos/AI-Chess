using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BContents",
                columns: table => new
                {
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    To = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BContents", x => new { x.Position, x.To });
                });

            migrationBuilder.CreateTable(
                name: "WContents",
                columns: table => new
                {
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    From = table.Column<int>(type: "INTEGER", nullable: false),
                    To = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WContents", x => new { x.Position, x.To, x.From });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BContents");

            migrationBuilder.DropTable(
                name: "WContents");
        }
    }
}
