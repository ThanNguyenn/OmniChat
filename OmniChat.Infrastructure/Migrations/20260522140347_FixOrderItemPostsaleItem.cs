using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderItemPostsaleItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostSaleItems_OrderItemId",
                table: "PostSaleItems");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleItems_OrderItemId",
                table: "PostSaleItems",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PostSaleItems_OrderItemId",
                table: "PostSaleItems");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleItems_OrderItemId",
                table: "PostSaleItems",
                column: "OrderItemId",
                unique: true);
        }
    }
}
