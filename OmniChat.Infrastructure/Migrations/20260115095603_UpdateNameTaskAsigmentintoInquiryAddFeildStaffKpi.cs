using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameTaskAsigmentintoInquiryAddFeildStaffKpi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskAssignments");

            migrationBuilder.AddColumn<int>(
                name: "CurrentValue",
                table: "StaffKpis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Inquirys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquirys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inquirys_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquirys_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquirys_SupportConversations_SupportConversationId",
                        column: x => x.SupportConversationId,
                        principalTable: "SupportConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inquirys_DepartmentId_Status",
                table: "Inquirys",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Inquirys_StaffId_Status",
                table: "Inquirys",
                columns: new[] { "StaffId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Inquirys_SupportConversationId",
                table: "Inquirys",
                column: "SupportConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquirys_SupportConversationId_IsActive",
                table: "Inquirys",
                columns: new[] { "SupportConversationId", "IsActive" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inquirys");

            migrationBuilder.DropColumn(
                name: "CurrentValue",
                table: "StaffKpis");

            migrationBuilder.CreateTable(
                name: "TaskAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedType = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_SupportConversations_SupportConversationId",
                        column: x => x.SupportConversationId,
                        principalTable: "SupportConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_DepartmentId_Status",
                table: "TaskAssignments",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_StaffId_Status",
                table: "TaskAssignments",
                columns: new[] { "StaffId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_SupportConversationId",
                table: "TaskAssignments",
                column: "SupportConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_SupportConversationId_IsActive",
                table: "TaskAssignments",
                columns: new[] { "SupportConversationId", "IsActive" },
                unique: true);
        }
    }
}
