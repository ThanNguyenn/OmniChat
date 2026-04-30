using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ActionByAuditBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchAudits_Staffs_ActionById",
                table: "BatchAudits");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchAudits_Staffs_ActionById",
                table: "BatchAudits",
                column: "ActionById",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BatchAudits_Staffs_ActionById",
                table: "BatchAudits");

            migrationBuilder.AddForeignKey(
                name: "FK_BatchAudits_Staffs_ActionById",
                table: "BatchAudits",
                column: "ActionById",
                principalTable: "Staffs",
                principalColumn: "Id");
        }
    }
}
