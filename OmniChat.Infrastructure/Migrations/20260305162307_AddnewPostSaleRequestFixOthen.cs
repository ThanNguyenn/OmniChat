using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddnewPostSaleRequestFixOthen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                table: "ZaloOathTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActionById",
                table: "TaskAssignmentHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ActionToId",
                table: "TaskAssignmentHistories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                table: "InstagramOathTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                table: "FacebookOathTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PostSaleRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PresentByStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    FraudFlag = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    ResolveById = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostSaleRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostSaleRequests_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostSaleRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostSaleRequests_Staffs_PresentByStaffId",
                        column: x => x.PresentByStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostSaleRequests_Staffs_ResolveById",
                        column: x => x.ResolveById,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZaloOathTokens_ProviderId",
                table: "ZaloOathTokens",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentHistories_ActionById",
                table: "TaskAssignmentHistories",
                column: "ActionById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentHistories_ActionToId",
                table: "TaskAssignmentHistories",
                column: "ActionToId");

            migrationBuilder.CreateIndex(
                name: "IX_InstagramOathTokens_ProviderId",
                table: "InstagramOathTokens",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_FacebookOathTokens_ProviderId",
                table: "FacebookOathTokens",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleRequests_CustomerId",
                table: "PostSaleRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleRequests_OrderId",
                table: "PostSaleRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleRequests_PresentByStaffId",
                table: "PostSaleRequests",
                column: "PresentByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSaleRequests_ResolveById",
                table: "PostSaleRequests",
                column: "ResolveById");

            migrationBuilder.AddForeignKey(
                name: "FK_FacebookOathTokens_Providers_ProviderId",
                table: "FacebookOathTokens",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstagramOathTokens_Providers_ProviderId",
                table: "InstagramOathTokens",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignmentHistories_Staffs_ActionById",
                table: "TaskAssignmentHistories",
                column: "ActionById",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskAssignmentHistories_Staffs_ActionToId",
                table: "TaskAssignmentHistories",
                column: "ActionToId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ZaloOathTokens_Providers_ProviderId",
                table: "ZaloOathTokens",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacebookOathTokens_Providers_ProviderId",
                table: "FacebookOathTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_InstagramOathTokens_Providers_ProviderId",
                table: "InstagramOathTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignmentHistories_Staffs_ActionById",
                table: "TaskAssignmentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskAssignmentHistories_Staffs_ActionToId",
                table: "TaskAssignmentHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_ZaloOathTokens_Providers_ProviderId",
                table: "ZaloOathTokens");

            migrationBuilder.DropTable(
                name: "PostSaleRequests");

            migrationBuilder.DropIndex(
                name: "IX_ZaloOathTokens_ProviderId",
                table: "ZaloOathTokens");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignmentHistories_ActionById",
                table: "TaskAssignmentHistories");

            migrationBuilder.DropIndex(
                name: "IX_TaskAssignmentHistories_ActionToId",
                table: "TaskAssignmentHistories");

            migrationBuilder.DropIndex(
                name: "IX_InstagramOathTokens_ProviderId",
                table: "InstagramOathTokens");

            migrationBuilder.DropIndex(
                name: "IX_FacebookOathTokens_ProviderId",
                table: "FacebookOathTokens");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "ZaloOathTokens");

            migrationBuilder.DropColumn(
                name: "ActionById",
                table: "TaskAssignmentHistories");

            migrationBuilder.DropColumn(
                name: "ActionToId",
                table: "TaskAssignmentHistories");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "InstagramOathTokens");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "FacebookOathTokens");
        }
    }
}
