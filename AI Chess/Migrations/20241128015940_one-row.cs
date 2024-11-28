using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class onerow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "ZContents",
                newName: "Values");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "WContents",
                newName: "Values");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "AContents",
                newName: "Values");

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "ZContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "WContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "AContents",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Row",
                table: "ZContents");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "WContents");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "AContents");

            migrationBuilder.RenameColumn(
                name: "Values",
                table: "ZContents",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Values",
                table: "WContents",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Values",
                table: "AContents",
                newName: "Value");
        }
    }
}
