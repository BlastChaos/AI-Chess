using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class use_conversion_b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BContents",
                table: "BContents");

            migrationBuilder.RenameColumn(
                name: "To",
                table: "BContents",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "BContents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "BContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BContents",
                table: "BContents",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BContents_Position",
                table: "BContents",
                column: "Position",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BContents",
                table: "BContents");

            migrationBuilder.DropIndex(
                name: "IX_BContents_Position",
                table: "BContents");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BContents",
                newName: "To");

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "BContents",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "To",
                table: "BContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BContents",
                table: "BContents",
                columns: new[] { "Position", "To" });
        }
    }
}
