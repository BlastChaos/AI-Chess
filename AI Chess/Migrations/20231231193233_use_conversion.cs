using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class use_conversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ZContents",
                table: "ZContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WContents",
                table: "WContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AContents",
                table: "AContents");

            migrationBuilder.DropColumn(
                name: "To",
                table: "ZContents");

            migrationBuilder.DropColumn(
                name: "To",
                table: "WContents");

            migrationBuilder.DropColumn(
                name: "To",
                table: "AContents");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "ZContents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "WContents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "From",
                table: "AContents",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "ZContents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ZContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "WContents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "WContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AContents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZContents",
                table: "ZContents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WContents",
                table: "WContents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AContents",
                table: "AContents",
                column: "Id");

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
                name: "IX_AContents_Position",
                table: "AContents",
                column: "Position",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ZContents",
                table: "ZContents");

            migrationBuilder.DropIndex(
                name: "IX_ZContents_Position",
                table: "ZContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WContents",
                table: "WContents");

            migrationBuilder.DropIndex(
                name: "IX_WContents_Position",
                table: "WContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AContents",
                table: "AContents");

            migrationBuilder.DropIndex(
                name: "IX_AContents_Position",
                table: "AContents");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ZContents",
                newName: "From");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "WContents",
                newName: "From");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AContents",
                newName: "From");

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "ZContents",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "From",
                table: "ZContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "To",
                table: "ZContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "WContents",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "From",
                table: "WContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "To",
                table: "WContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "Value",
                table: "AContents",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "From",
                table: "AContents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "To",
                table: "AContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ZContents",
                table: "ZContents",
                columns: new[] { "Position", "To", "From" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_WContents",
                table: "WContents",
                columns: new[] { "Position", "To", "From" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AContents",
                table: "AContents",
                columns: new[] { "Position", "To", "From" });
        }
    }
}
