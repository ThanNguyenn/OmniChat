using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerOrderAndPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Keywords_KeywordTypes_KeyWordTypeId",
                table: "Keywords");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_KeywordTypes_KeyWordTypeId",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTasks_KeywordTypes_KeywordTypeId",
                table: "SupportTasks");

            migrationBuilder.DropTable(
                name: "BillingItems");

            migrationBuilder.DropTable(
                name: "MessageKeywords");

            migrationBuilder.DropTable(
                name: "TaskAssignmentHistories");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "MessageKeywordTypes");

            migrationBuilder.DropTable(
                name: "KeywordTypes");

            migrationBuilder.RenameColumn(
                name: "KeywordTypeId",
                table: "SupportTasks",
                newName: "IntentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTasks_KeywordTypeId",
                table: "SupportTasks",
                newName: "IX_SupportTasks_IntentTypeId");

            migrationBuilder.RenameColumn(
                name: "KeyWordTypeId",
                table: "Staffs",
                newName: "IntentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Staffs_KeyWordTypeId",
                table: "Staffs",
                newName: "IX_Staffs_IntentTypeId");

            migrationBuilder.RenameColumn(
                name: "KeyWordTypeId",
                table: "Keywords",
                newName: "IntentTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Keywords_KeyWordTypeId",
                table: "Keywords",
                newName: "IX_Keywords_IntentTypeId");

            migrationBuilder.AddColumn<int>(
                name: "TaskPiority",
                table: "SupportTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CloseAt",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstResponseAt",
                table: "SupportConversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "DriverId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "IntentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TypeName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    IntentTypePiority = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<double>(type: "double precision", nullable: false),
                    InvoiceStatus = table.Column<string>(type: "text", nullable: false),
                    InvoiceMethod = table.Column<string>(type: "text", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "false")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffPerformances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskCompleted = table.Column<int>(type: "integer", nullable: false),
                    AvgTaskHandleTime = table.Column<int>(type: "integer", nullable: false),
                    ConversationOwned = table.Column<int>(type: "integer", nullable: false),
                    AvgFirstResponseTime = table.Column<int>(type: "integer", nullable: false),
                    ReassignmentCount = table.Column<int>(type: "integer", nullable: false),
                    CancelledCount = table.Column<int>(type: "integer", nullable: false),
                    FromTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ToTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffPerformances_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SupportTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActionById = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionToId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskActions_Staffs_ActionById",
                        column: x => x.ActionById,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskActions_Staffs_ActionToId",
                        column: x => x.ActionToId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskActions_SupportTasks_SupportTaskId",
                        column: x => x.SupportTaskId,
                        principalTable: "SupportTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskCancelReasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SupportTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CancelledByStaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskCancelReasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskCancelReasons_SupportTasks_SupportTaskId",
                        column: x => x.SupportTaskId,
                        principalTable: "SupportTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Amount = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageIntentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageIntentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageIntentTypes_CustomerMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "CustomerMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageIntentTypes_IntentTypes_IntentTypeId",
                        column: x => x.IntentTypeId,
                        principalTable: "IntentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffIntentTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffIntentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffIntentTypes_IntentTypes_IntentTypeId",
                        column: x => x.IntentTypeId,
                        principalTable: "IntentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffIntentTypes_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Total = table.Column<double>(type: "double precision", nullable: false),
                    CreditNoteStatus = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allocations_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Allocations_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DriverId",
                table: "Orders",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InvoiceId",
                table: "Orders",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_Id",
                table: "Allocations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_InvoiceId",
                table: "Allocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_WalletId",
                table: "Allocations",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_InvoiceId",
                table: "CreditNotes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_OrderId",
                table: "CreditNotes",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_IntentTypes_TypeName",
                table: "IntentTypes",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageIntentTypes_IntentTypeId",
                table: "MessageIntentTypes",
                column: "IntentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageIntentTypes_MessageId",
                table: "MessageIntentTypes",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffIntentTypes_IntentTypeId_StaffId",
                table: "StaffIntentTypes",
                columns: new[] { "IntentTypeId", "StaffId" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffIntentTypes_StaffId",
                table: "StaffIntentTypes",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPerformances_StaffId",
                table: "StaffPerformances",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActions_ActionById",
                table: "TaskActions",
                column: "ActionById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActions_ActionToId",
                table: "TaskActions",
                column: "ActionToId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActions_SupportTaskId",
                table: "TaskActions",
                column: "SupportTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskCancelReasons_SupportTaskId",
                table: "TaskCancelReasons",
                column: "SupportTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CustomerId",
                table: "Wallets",
                column: "CustomerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Keywords_IntentTypes_IntentTypeId",
                table: "Keywords",
                column: "IntentTypeId",
                principalTable: "IntentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Invoices_InvoiceId",
                table: "Orders",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Staffs_DriverId",
                table: "Orders",
                column: "DriverId",
                principalTable: "Staffs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_IntentTypes_IntentTypeId",
                table: "Staffs",
                column: "IntentTypeId",
                principalTable: "IntentTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTasks_IntentTypes_IntentTypeId",
                table: "SupportTasks",
                column: "IntentTypeId",
                principalTable: "IntentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Keywords_IntentTypes_IntentTypeId",
                table: "Keywords");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Invoices_InvoiceId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Staffs_DriverId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_IntentTypes_IntentTypeId",
                table: "Staffs");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTasks_IntentTypes_IntentTypeId",
                table: "SupportTasks");

            migrationBuilder.DropTable(
                name: "Allocations");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropTable(
                name: "MessageIntentTypes");

            migrationBuilder.DropTable(
                name: "StaffIntentTypes");

            migrationBuilder.DropTable(
                name: "StaffPerformances");

            migrationBuilder.DropTable(
                name: "TaskActions");

            migrationBuilder.DropTable(
                name: "TaskCancelReasons");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "IntentTypes");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DriverId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_InvoiceId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TaskPiority",
                table: "SupportTasks");

            migrationBuilder.DropColumn(
                name: "CloseAt",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "FirstResponseAt",
                table: "SupportConversations");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "IntentTypeId",
                table: "SupportTasks",
                newName: "KeywordTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTasks_IntentTypeId",
                table: "SupportTasks",
                newName: "IX_SupportTasks_KeywordTypeId");

            migrationBuilder.RenameColumn(
                name: "IntentTypeId",
                table: "Staffs",
                newName: "KeyWordTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Staffs_IntentTypeId",
                table: "Staffs",
                newName: "IX_Staffs_KeyWordTypeId");

            migrationBuilder.RenameColumn(
                name: "IntentTypeId",
                table: "Keywords",
                newName: "KeyWordTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Keywords_IntentTypeId",
                table: "Keywords",
                newName: "IX_Keywords_KeyWordTypeId");

            migrationBuilder.CreateTable(
                name: "KeywordTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    KeywordTypePiority = table.Column<int>(type: "integer", nullable: false),
                    TypeName = table.Column<string>(type: "text", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeywordTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PayMethod = table.Column<string>(type: "text", nullable: false),
                    PayStatus = table.Column<string>(type: "text", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_CustomerProfiles_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskAssignmentHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ActionById = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionToId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupportTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignmentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignmentHistories_Staffs_ActionById",
                        column: x => x.ActionById,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignmentHistories_Staffs_ActionToId",
                        column: x => x.ActionToId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskAssignmentHistories_SupportTasks_SupportTaskId",
                        column: x => x.SupportTaskId,
                        principalTable: "SupportTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageKeywordTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    KeywordTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "BillingItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    BillStatus = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillingItems_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageKeywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    KeywordId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageKeywordTypesId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageKeywords_Keywords_KeywordId",
                        column: x => x.KeywordId,
                        principalTable: "Keywords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageKeywords_MessageKeywordTypes_MessageKeywordTypesId",
                        column: x => x.MessageKeywordTypesId,
                        principalTable: "MessageKeywordTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_OrderId",
                table: "BillingItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_PaymentId",
                table: "BillingItems",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_KeywordTypes_TypeName",
                table: "KeywordTypes",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywords_KeywordId",
                table: "MessageKeywords",
                column: "KeywordId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywords_MessageKeywordTypesId",
                table: "MessageKeywords",
                column: "MessageKeywordTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywordTypes_KeywordTypeId",
                table: "MessageKeywordTypes",
                column: "KeywordTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageKeywordTypes_MessageId",
                table: "MessageKeywordTypes",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentHistories_ActionById",
                table: "TaskAssignmentHistories",
                column: "ActionById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentHistories_ActionToId",
                table: "TaskAssignmentHistories",
                column: "ActionToId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignmentHistories_SupportTaskId",
                table: "TaskAssignmentHistories",
                column: "SupportTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Keywords_KeywordTypes_KeyWordTypeId",
                table: "Keywords",
                column: "KeyWordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_KeywordTypes_KeyWordTypeId",
                table: "Staffs",
                column: "KeyWordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTasks_KeywordTypes_KeywordTypeId",
                table: "SupportTasks",
                column: "KeywordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
