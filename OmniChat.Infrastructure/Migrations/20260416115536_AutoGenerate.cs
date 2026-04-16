using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoGenerate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "KeywordCodeSeq");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Keywords",
                type: "text",
                nullable: false,
                defaultValueSql: "'KW' || LPAD(nextval('\"KeywordCodeSeq\"')::text, 6, '0')",
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "KeywordCodeSeq");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Keywords",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'KW' || LPAD(nextval('\"KeywordCodeSeq\"')::text, 6, '0')");
        }
    }
}
