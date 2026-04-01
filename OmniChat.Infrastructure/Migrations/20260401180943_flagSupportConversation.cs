using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class flagSupportConversation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastCustomerMessageAt",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastStaffMessageAt",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderSent",
                table: "SupportConversations",
                type: "boolean",
                nullable: false,
                defaultValueSql: "false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCustomerMessageAt",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "LastStaffMessageAt",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "ReminderSent",
                table: "SupportConversations");
        }
    }
}
