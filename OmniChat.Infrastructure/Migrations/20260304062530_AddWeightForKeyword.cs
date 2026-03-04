using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightForKeyword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Weight",
                table: "Keywords",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Keywords");
        }
    }
}
