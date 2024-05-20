using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chess.Migrations
{
    /// <inheritdoc />
    public partial class AddNeuralNetworkId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NeuralNetworkId",
                table: "ZContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NeuralNetworkId",
                table: "WContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NeuralNetworkId",
                table: "BContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NeuralNetworkId",
                table: "AContents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeuralNetworkId",
                table: "ZContents");

            migrationBuilder.DropColumn(
                name: "NeuralNetworkId",
                table: "WContents");

            migrationBuilder.DropColumn(
                name: "NeuralNetworkId",
                table: "BContents");

            migrationBuilder.DropColumn(
                name: "NeuralNetworkId",
                table: "AContents");
        }
    }
}
