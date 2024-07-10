using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectingDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddingMigrationn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedAt",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeletedAt",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
