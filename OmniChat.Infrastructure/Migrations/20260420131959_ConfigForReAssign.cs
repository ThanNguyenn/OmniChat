using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigForReAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SupportConversationId",
                table: "Claims",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_SupportConversationId",
                table: "Claims",
                column: "SupportConversationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_SupportConversations_SupportConversationId",
                table: "Claims",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_SupportConversations_SupportConversationId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_SupportConversationId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "SupportConversationId",
                table: "Claims");
        }
    }
}
