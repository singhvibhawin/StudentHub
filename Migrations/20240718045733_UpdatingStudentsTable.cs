using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectingDatabase.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingStudentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePaths",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Documents",
                newName: "FilePaths");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePaths",
                table: "Documents",
                newName: "FilePath");

            migrationBuilder.AddColumn<string>(
                name: "FilePaths",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
