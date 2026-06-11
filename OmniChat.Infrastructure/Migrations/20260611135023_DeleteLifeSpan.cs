using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLifeSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LifeSpan",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LifeSpan",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
