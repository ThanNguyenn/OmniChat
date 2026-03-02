using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryCustoMessageFaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CustomerMessages_ConversationId_IsRead",
                table: "CustomerMessages",
                columns: new[] { "ConversationId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerMessages_ConversationId_IsRead",
                table: "CustomerMessages");
        }
    }
}
