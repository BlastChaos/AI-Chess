using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AContents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NeuralNetworkId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BContents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NeuralNetworkId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WContents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NeuralNetworkId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZContents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NeuralNetworkId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZContents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AContents");

            migrationBuilder.DropTable(
                name: "BContents");

            migrationBuilder.DropTable(
                name: "WContents");

            migrationBuilder.DropTable(
                name: "ZContents");
        }
    }
}
