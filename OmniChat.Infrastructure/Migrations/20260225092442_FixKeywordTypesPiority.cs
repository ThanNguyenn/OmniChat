using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixKeywordTypesPiority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Piority",
                table: "Keywords");

            migrationBuilder.AddColumn<int>(
                name: "KeywordTypePiority",
                table: "KeywordTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeywordTypePiority",
                table: "KeywordTypes");

            migrationBuilder.AddColumn<int>(
                name: "Piority",
                table: "Keywords",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
