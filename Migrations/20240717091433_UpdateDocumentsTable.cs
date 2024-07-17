using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConnectingDatabase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "FilePaths",
                table: "Documents",
                newName: "FilePath");

            migrationBuilder.AddColumn<string>(
                name: "DocumentName",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentName",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Documents",
                newName: "FilePaths");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Documents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
