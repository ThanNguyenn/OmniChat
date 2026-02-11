using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductLifeSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LifeSpan",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SupportConversations_ActiveCustomerId_ProvidersId",
                table: "SupportConversations",
                columns: new[] { "ActiveCustomerId", "ProvidersId" },
                unique: true,
                filter: "\"Status\" = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SupportConversations_ActiveCustomerId_ProvidersId",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "LifeSpan",
                table: "Products");
        }
    }
}
