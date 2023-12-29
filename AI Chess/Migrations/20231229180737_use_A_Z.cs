using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class use_A_Z : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AContents",
                columns: table => new
                {
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    From = table.Column<int>(type: "INTEGER", nullable: false),
                    To = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AContents", x => new { x.Position, x.To, x.From });
                });

            migrationBuilder.CreateTable(
                name: "ZContents",
                columns: table => new
                {
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    From = table.Column<int>(type: "INTEGER", nullable: false),
                    To = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZContents", x => new { x.Position, x.To, x.From });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AContents");

            migrationBuilder.DropTable(
                name: "ZContents");
        }
    }
}
