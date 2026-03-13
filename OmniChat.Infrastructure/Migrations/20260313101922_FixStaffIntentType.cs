using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStaffIntentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_IntentTypes_IntentTypeId",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_IntentTypeId",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "IntentTypeId",
                table: "Staffs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IntentTypeId",
                table: "Staffs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_IntentTypeId",
                table: "Staffs",
                column: "IntentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_IntentTypes_IntentTypeId",
                table: "Staffs",
                column: "IntentTypeId",
                principalTable: "IntentTypes",
                principalColumn: "Id");
        }
    }
}
