using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UsingEfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoPropertyToRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVet",
                table: "recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "IsVet",
                table: "recipes");
        }
    }
}
