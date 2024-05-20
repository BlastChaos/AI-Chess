using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class NoPostionUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ZContents_Position",
                table: "ZContents");

            migrationBuilder.DropIndex(
                name: "IX_WContents_Position",
                table: "WContents");

            migrationBuilder.DropIndex(
                name: "IX_BContents_Position",
                table: "BContents");

            migrationBuilder.DropIndex(
                name: "IX_AContents_Position",
                table: "AContents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ZContents_Position",
                table: "ZContents",
                column: "Position",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WContents_Position",
                table: "WContents",
                column: "Position",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BContents_Position",
                table: "BContents",
                column: "Position",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AContents_Position",
                table: "AContents",
                column: "Position",
                unique: true);
        }
    }
}
