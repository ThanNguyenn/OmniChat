using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OmniChatDBV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Departments_DepartmentId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfiles_Providers_ProvidersId",
                table: "CustomerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageKeywords_CustomerMessages_CustomerMessageId",
                table: "MessageKeywords");

            migrationBuilder.DropForeignKey(
                name: "FK_RefeshTokens_Accounts_AccountId",
                table: "RefeshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DepartmentConversationFiles");

            migrationBuilder.DropTable(
                name: "DepartmentKeywords");

            migrationBuilder.DropTable(
                name: "DepartmentStaffMessages");

            migrationBuilder.DropTable(
                name: "Inquiries");

            migrationBuilder.DropTable(
                name: "StaffKpis");

            migrationBuilder.DropTable(
                name: "StaffShifts");

            migrationBuilder.DropTable(
                name: "DepartmentConversations");

            migrationBuilder.DropTable(
                name: "Kpis");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "DepartmentConversationTypes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_MessageKeywords_KeywordId_CustomerMessageId",
                table: "MessageKeywords");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_ProvidersId",
                table: "CustomerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefeshTokens",
                table: "RefeshTokens");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "ProvidersId",
                table: "CustomerProfiles");

            migrationBuilder.RenameTable(
                name: "RefeshTokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "AvartarUrl",
                table: "SupportConversations",
                newName: "AvatarUrl");

            migrationBuilder.RenameColumn(
                name: "CustomerMessageId",
                table: "MessageKeywords",
                newName: "MessageKeywordTypesId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageKeywords_CustomerMessageId",
                table: "MessageKeywords",
                newName: "IX_MessageKeywords_MessageKeywordTypesId");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "CustomerProfiles",
                newName: "ZaloSenderId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Claims",
                newName: "KeywordTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_DepartmentId",
                table: "Claims",
                newName: "IX_Claims_KeywordTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_RefeshTokens_AccountId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_AccountId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ZaloOathTokens",
                type: "boolean",
                nullable: false,
                defaultValueSql: "true",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDate",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "KeyWordTypeId",
                table: "Keywords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "CustomerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CustomerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FacebookSenderId",
                table: "CustomerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstagramSenderId",
                table: "CustomerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CustomerProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "ConversationFiles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "FacebookOathTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    AccessTokenExpiredDate = table.Column<string>(type: "text", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacebookOathTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstagramOathTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AccessToken = table.Column<string>(type: "text", nullable: false),
                    AccessTokenExpiredDate = table.Column<string>(type: "text", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramOathTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternalConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConversationName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeywordTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TypeName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeywordTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderDate = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalAmount = table.Column<double>(type: "double precision", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProductPackagingType = table.Column<string>(type: "text", nullable: false),
                    VolumeMl = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true"),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InternalConversationFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConversationFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    InternalConversationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalConversationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalConversationFiles_ConversationFiles_ConversationFil~",
                        column: x => x.ConversationFileId,
                        principalTable: "ConversationFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalConversationFiles_InternalConversations_InternalCon~",
                        column: x => x.InternalConversationId,
                        principalTable: "InternalConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalStaffMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    InternalConversationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalStaffMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalStaffMessages_InternalConversations_InternalConvers~",
                        column: x => x.InternalConversationId,
                        principalTable: "InternalConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InternalStaffMessages_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageKeywordTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeywordTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageKeywordTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageKeywordTypes_CustomerMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "CustomerMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageKeywordTypes_KeywordTypes_KeywordTypeId",
                        column: x => x.KeywordTypeId,
                        principalTable: "KeywordTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SupportConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeywordTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CurrentAssignedStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompleteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTasks_KeywordTypes_KeywordTypeId",
                        column: x => x.KeywordTypeId,
                        principalTable: "KeywordTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTasks_Staffs_CurrentAssignedStaffId",
                        column: x => x.CurrentAssignedStaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTasks_SupportConversations_SupportConversationId",
                        column: x => x.SupportConversationId,
                        principalTable: "SupportConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManuFactureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "true")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBatches_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignmentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SupportTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignmentHistories_SupportTasks_SupportTaskId",
                        column: x => x.SupportTaskId,
                        principalTable: "SupportTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_ProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportConversations_ProvidersId",
                table: "SupportConversations",
                column: "ProvidersId");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Id",
                table: "Providers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_KeywordText",
                table: "Keywords",
                column: "KeywordText");

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_KeyWordTypeId",
                table: "Keywords",
                column: "KeyWordTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_Email",
                table: "CustomerProfiles",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_PhoneNumber",
                table: "CustomerProfiles",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacebookOathTokens_IsActive",
                table: "FacebookOathTokens",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InstagramOathTokens_IsActive",
                table: "InstagramOathTokens",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InternalConversationFiles_ConversationFileId",
                table: "InternalConversationFiles",
                column: "ConversationFileId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalConversationFiles_InternalConversationId",
                table: "InternalConversationFiles",
                column: "InternalConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStaffMessages_InternalConversationId",
                table: "InternalStaffMessages",
                column: "InternalConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalStaffMessages_StaffId",
                table: "InternalStaffMessages",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_KeywordTypes_TypeName",
                table: "KeywordTypes",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywordTypes_KeywordTypeId",
                table: "MessageKeywordTypes",
                column: "KeywordTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywordTypes_MessageId",
                table: "MessageKeywordTypes",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductBatchId",
                table: "OrderItems",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveryStatus",
                table: "Orders",
                column: "DeliveryStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_ProductId",
                table: "ProductBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTasks_CurrentAssignedStaffId",
                table: "SupportTasks",
                column: "CurrentAssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTasks_KeywordTypeId",
                table: "SupportTasks",
                column: "KeywordTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTasks_Status",
                table: "SupportTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTasks_SupportConversationId",
                table: "SupportTasks",
                column: "SupportConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentHistories_SupportTaskId",
                table: "TaskAssignmentHistories",
                column: "SupportTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_KeywordTypes_KeywordTypeId",
                table: "Claims",
                column: "KeywordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Keywords_KeywordTypes_KeyWordTypeId",
                table: "Keywords",
                column: "KeyWordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageKeywords_MessageKeywordTypes_MessageKeywordTypesId",
                table: "MessageKeywords",
                column: "MessageKeywordTypesId",
                principalTable: "MessageKeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Accounts_AccountId",
                table: "RefreshTokens",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_KeywordTypes_KeywordTypeId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Keywords_KeywordTypes_KeyWordTypeId",
                table: "Keywords");

            migrationBuilder.DropForeignKey(
                name: "FK_MessageKeywords_MessageKeywordTypes_MessageKeywordTypesId",
                table: "MessageKeywords");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Accounts_AccountId",
                table: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "FacebookOathTokens");

            migrationBuilder.DropTable(
                name: "InstagramOathTokens");

            migrationBuilder.DropTable(
                name: "InternalConversationFiles");

            migrationBuilder.DropTable(
                name: "InternalStaffMessages");

            migrationBuilder.DropTable(
                name: "MessageKeywordTypes");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "TaskAssignmentHistories");

            migrationBuilder.DropTable(
                name: "InternalConversations");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ProductBatches");

            migrationBuilder.DropTable(
                name: "SupportTasks");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "KeywordTypes");

            migrationBuilder.DropIndex(
                name: "IX_SupportConversations_ProvidersId",
                table: "SupportConversations");

            migrationBuilder.DropIndex(
                name: "IX_Providers_Id",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Keywords_KeywordText",
                table: "Keywords");

            migrationBuilder.DropIndex(
                name: "IX_Keywords_KeyWordTypeId",
                table: "Keywords");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_Email",
                table: "CustomerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfiles_PhoneNumber",
                table: "CustomerProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "KeyWordTypeId",
                table: "Keywords");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "FacebookSenderId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "InstagramSenderId",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CustomerProfiles");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefeshTokens");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "SupportConversations",
                newName: "AvartarUrl");

            migrationBuilder.RenameColumn(
                name: "MessageKeywordTypesId",
                table: "MessageKeywords",
                newName: "CustomerMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_MessageKeywords_MessageKeywordTypesId",
                table: "MessageKeywords",
                newName: "IX_MessageKeywords_CustomerMessageId");

            migrationBuilder.RenameColumn(
                name: "ZaloSenderId",
                table: "CustomerProfiles",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "KeywordTypeId",
                table: "Claims",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_KeywordTypeId",
                table: "Claims",
                newName: "IX_Claims_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_AccountId",
                table: "RefeshTokens",
                newName: "IX_RefeshTokens_AccountId");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "ZaloOathTokens",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "true");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdateDate",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Staffs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Staffs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProvidersId",
                table: "CustomerProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                table: "ConversationFiles",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefeshTokens",
                table: "RefeshTokens",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NewData = table.Column<string>(type: "jsonb", nullable: false),
                    OldData = table.Column<string>(type: "jsonb", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentConversationTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    TypeName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentConversationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DepartmentName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentConversationTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationName = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentConversations_DepartmentConversationTypes_Departm~",
                        column: x => x.DepartmentConversationTypeId,
                        principalTable: "DepartmentConversationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentConversations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentKeywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeywordId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentKeywords_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentKeywords_Keywords_KeywordId",
                        column: x => x.KeywordId,
                        principalTable: "Keywords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inquiries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inquiries_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_SupportConversations_SupportConversationId",
                        column: x => x.SupportConversationId,
                        principalTable: "SupportConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchivedValue = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false"),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kpis_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentConversationFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConversationFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentConversationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentConversationFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentConversationFiles_ConversationFiles_ConversationF~",
                        column: x => x.ConversationFileId,
                        principalTable: "ConversationFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DepartmentConversationFiles_DepartmentConversations_Departm~",
                        column: x => x.DepartmentConversationId,
                        principalTable: "DepartmentConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentStaffMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepartmentConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentStaffMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentStaffMessages_DepartmentConversations_DepartmentC~",
                        column: x => x.DepartmentConversationId,
                        principalTable: "DepartmentConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepartmentStaffMessages_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    KpiId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentValue = table.Column<int>(type: "integer", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffKpis_Kpis_KpiId",
                        column: x => x.KpiId,
                        principalTable: "Kpis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffKpis_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffShifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffShifts_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffShifts_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_DepartmentId",
                table: "Staffs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywords_KeywordId_CustomerMessageId",
                table: "MessageKeywords",
                columns: new[] { "KeywordId", "CustomerMessageId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_ProvidersId",
                table: "CustomerProfiles",
                column: "ProvidersId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreateDate",
                table: "AuditLogs",
                column: "CreateDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentConversationFiles_ConversationFileId",
                table: "DepartmentConversationFiles",
                column: "ConversationFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentConversationFiles_DepartmentConversationId_Conver~",
                table: "DepartmentConversationFiles",
                columns: new[] { "DepartmentConversationId", "ConversationFileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentConversations_DepartmentConversationTypeId",
                table: "DepartmentConversations",
                column: "DepartmentConversationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentConversations_DepartmentId_DepartmentConversation~",
                table: "DepartmentConversations",
                columns: new[] { "DepartmentId", "DepartmentConversationTypeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentKeywords_DepartmentId",
                table: "DepartmentKeywords",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentKeywords_KeywordId",
                table: "DepartmentKeywords",
                column: "KeywordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentStaffMessages_DepartmentConversationId_Status",
                table: "DepartmentStaffMessages",
                columns: new[] { "DepartmentConversationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentStaffMessages_StaffId",
                table: "DepartmentStaffMessages",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_DepartmentId_Status",
                table: "Inquiries",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_StaffId_Status",
                table: "Inquiries",
                columns: new[] { "StaffId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_SupportConversationId",
                table: "Inquiries",
                column: "SupportConversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_SupportConversationId_IsActive",
                table: "Inquiries",
                columns: new[] { "SupportConversationId", "IsActive" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kpis_DepartmentId",
                table: "Kpis",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_DepartmentId",
                table: "Shifts",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffKpis_KpiId_Status",
                table: "StaffKpis",
                columns: new[] { "KpiId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffKpis_StaffId_KpiId",
                table: "StaffKpis",
                columns: new[] { "StaffId", "KpiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffKpis_StaffId_Status",
                table: "StaffKpis",
                columns: new[] { "StaffId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_ShiftId_Status",
                table: "StaffShifts",
                columns: new[] { "ShiftId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_StaffId_ShiftId",
                table: "StaffShifts",
                columns: new[] { "StaffId", "ShiftId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_Status",
                table: "StaffShifts",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Departments_DepartmentId",
                table: "Claims",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfiles_Providers_ProvidersId",
                table: "CustomerProfiles",
                column: "ProvidersId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MessageKeywords_CustomerMessages_CustomerMessageId",
                table: "MessageKeywords",
                column: "CustomerMessageId",
                principalTable: "CustomerMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefeshTokens_Accounts_AccountId",
                table: "RefeshTokens",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_Departments_DepartmentId",
                table: "Staffs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
