using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerMessages_supportConversations_ConversationId",
                table: "CustomerMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentConversations_departmentConversationTypes_Departm~",
                table: "DepartmentConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_supportConversations_SupportConversationId",
                table: "FeedBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_supportConversations_ConversationId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversationFiles_supportConversations_SupportConver~",
                table: "SupportConversationFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_supportConversations_CustomerProfiles_ActiveCustomerId",
                table: "supportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_supportConversations_Providers_ProvidersId",
                table: "supportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_supportConversations_Staffs_ActiveStaffId",
                table: "supportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportStaffMessages_supportConversations_SupportConversati~",
                table: "SupportStaffMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_supportConversations_SupportConversationId",
                table: "TaskAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_supportConversations",
                table: "supportConversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_departmentConversationTypes",
                table: "departmentConversationTypes");

            migrationBuilder.RenameTable(
                name: "supportConversations",
                newName: "SupportConversations");

            migrationBuilder.RenameTable(
                name: "departmentConversationTypes",
                newName: "DepartmentConversationTypes");

            migrationBuilder.RenameIndex(
                name: "IX_supportConversations_ProvidersId_Status",
                table: "SupportConversations",
                newName: "IX_SupportConversations_ProvidersId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_supportConversations_ActiveStaffId_Status",
                table: "SupportConversations",
                newName: "IX_SupportConversations_ActiveStaffId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_supportConversations_ActiveCustomerId",
                table: "SupportConversations",
                newName: "IX_SupportConversations_ActiveCustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportConversations",
                table: "SupportConversations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DepartmentConversationTypes",
                table: "DepartmentConversationTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerMessages_SupportConversations_ConversationId",
                table: "CustomerMessages",
                column: "ConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentConversations_DepartmentConversationTypes_Departm~",
                table: "DepartmentConversations",
                column: "DepartmentConversationTypeId",
                principalTable: "DepartmentConversationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBacks_SupportConversations_SupportConversationId",
                table: "FeedBacks",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_SupportConversations_ConversationId",
                table: "Notifications",
                column: "ConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversationFiles_SupportConversations_SupportConver~",
                table: "SupportConversationFiles",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_CustomerProfiles_ActiveCustomerId",
                table: "SupportConversations",
                column: "ActiveCustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Providers_ProvidersId",
                table: "SupportConversations",
                column: "ProvidersId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversations_Staffs_ActiveStaffId",
                table: "SupportConversations",
                column: "ActiveStaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportStaffMessages_SupportConversations_SupportConversati~",
                table: "SupportStaffMessages",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_SupportConversations_SupportConversationId",
                table: "TaskAssignments",
                column: "SupportConversationId",
                principalTable: "SupportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerMessages_SupportConversations_ConversationId",
                table: "CustomerMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentConversations_DepartmentConversationTypes_Departm~",
                table: "DepartmentConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedBacks_SupportConversations_SupportConversationId",
                table: "FeedBacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_SupportConversations_ConversationId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversationFiles_SupportConversations_SupportConver~",
                table: "SupportConversationFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_CustomerProfiles_ActiveCustomerId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Providers_ProvidersId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportConversations_Staffs_ActiveStaffId",
                table: "SupportConversations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportStaffMessages_SupportConversations_SupportConversati~",
                table: "SupportStaffMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignments_SupportConversations_SupportConversationId",
                table: "TaskAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportConversations",
                table: "SupportConversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DepartmentConversationTypes",
                table: "DepartmentConversationTypes");

            migrationBuilder.RenameTable(
                name: "SupportConversations",
                newName: "supportConversations");

            migrationBuilder.RenameTable(
                name: "DepartmentConversationTypes",
                newName: "departmentConversationTypes");

            migrationBuilder.RenameIndex(
                name: "IX_SupportConversations_ProvidersId_Status",
                table: "supportConversations",
                newName: "IX_supportConversations_ProvidersId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_SupportConversations_ActiveStaffId_Status",
                table: "supportConversations",
                newName: "IX_supportConversations_ActiveStaffId_Status");

            migrationBuilder.RenameIndex(
                name: "IX_SupportConversations_ActiveCustomerId",
                table: "supportConversations",
                newName: "IX_supportConversations_ActiveCustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_supportConversations",
                table: "supportConversations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_departmentConversationTypes",
                table: "departmentConversationTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerMessages_supportConversations_ConversationId",
                table: "CustomerMessages",
                column: "ConversationId",
                principalTable: "supportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepartmentConversations_departmentConversationTypes_Departm~",
                table: "DepartmentConversations",
                column: "DepartmentConversationTypeId",
                principalTable: "departmentConversationTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedBacks_supportConversations_SupportConversationId",
                table: "FeedBacks",
                column: "SupportConversationId",
                principalTable: "supportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_supportConversations_ConversationId",
                table: "Notifications",
                column: "ConversationId",
                principalTable: "supportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportConversationFiles_supportConversations_SupportConver~",
                table: "SupportConversationFiles",
                column: "SupportConversationId",
                principalTable: "supportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_supportConversations_CustomerProfiles_ActiveCustomerId",
                table: "supportConversations",
                column: "ActiveCustomerId",
                principalTable: "CustomerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_supportConversations_Providers_ProvidersId",
                table: "supportConversations",
                column: "ProvidersId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_supportConversations_Staffs_ActiveStaffId",
                table: "supportConversations",
                column: "ActiveStaffId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportStaffMessages_supportConversations_SupportConversati~",
                table: "SupportStaffMessages",
                column: "SupportConversationId",
                principalTable: "supportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignments_supportConversations_SupportConversationId",
                table: "TaskAssignments",
                column: "SupportConversationId",
                principalTable: "supportConversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
