using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixInquiryName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquirys_Departments_DepartmentId",
                table: "Inquirys");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquirys_Staffs_StaffId",
                table: "Inquirys");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquirys_SupportConversations_SupportConversationId",
                table: "Inquirys");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inquirys",
                table: "Inquirys");

            migrationBuilder.RenameTable(
                name: "Inquirys",
                newName: "Inquiries");

            migrationBuilder.RenameIndex(
                name: "IX_Inquirys_SupportConversationId_IsActive",
                table: "Inquiries",
                newName: "IX_Inquiries_SupportConversationId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Inquirys_SupportConversationId",
                table: "Inquiries",
                newName: "IX_Inquiries_SupportConversationId");

            migrationBuilder.RenameIndex(
                name: "IX_Inquirys_StaffId_Status",
                table: "Inquiries",
                newName: "IX_Inquiries_StaffId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Inquirys_DepartmentId_Status",
                table: "Inquiries",
                newName: "IX_Inquiries_DepartmentId_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inquiries",
                table: "Inquiries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Departments_DepartmentId",
                table: "Inquiries",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_Staffs_StaffId",
                table: "Inquiries",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquiries_SupportConversations_SupportConversationId",
                table: "Inquiries",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Departments_DepartmentId",
                table: "Inquiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_Staffs_StaffId",
                table: "Inquiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Inquiries_SupportConversations_SupportConversationId",
                table: "Inquiries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inquiries",
                table: "Inquiries");

            migrationBuilder.RenameTable(
                name: "Inquiries",
                newName: "Inquirys");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_SupportConversationId_IsActive",
                table: "Inquirys",
                newName: "IX_Inquirys_SupportConversationId_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_SupportConversationId",
                table: "Inquirys",
                newName: "IX_Inquirys_SupportConversationId");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_StaffId_Status",
                table: "Inquirys",
                newName: "IX_Inquirys_StaffId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Inquiries_DepartmentId_Status",
                table: "Inquirys",
                newName: "IX_Inquirys_DepartmentId_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inquirys",
                table: "Inquirys",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inquirys_Departments_DepartmentId",
                table: "Inquirys",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquirys_Staffs_StaffId",
                table: "Inquirys",
                column: "StaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inquirys_SupportConversations_SupportConversationId",
                table: "Inquirys",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
