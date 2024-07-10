using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectingDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathsToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePaths",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePaths",
                table: "Students");
        }
    }
}
