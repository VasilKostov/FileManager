using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileManager.DB.Migrations
{
    /// <inheritdoc />
    public partial class FilePathadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "FileRecords",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "FileRecords");
        }
    }
}
