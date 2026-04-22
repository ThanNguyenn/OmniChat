using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductBatchFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternalConversationFiles");

            migrationBuilder.DropTable(
                name: "InternalStaffMessages");

            migrationBuilder.DropTable(
                name: "InternalConversations");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"ProductCodeSeq\" CASCADE;");

            migrationBuilder.CreateSequence<int>(
                name: "BatchCodeSeq");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'SP' || LPAD(nextval('\"ProductCodeSeq\"')::text, 6, '0')");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ProductBatches",
                type: "text",
                nullable: false,
                defaultValueSql: "'LOT' || LPAD(nextval('\"BatchCodeSeq\"')::text, 6, '0')",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ProductBatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsExpired",
                table: "ProductBatches",
                type: "boolean",
                nullable: true,
                defaultValueSql: "false");

            migrationBuilder.CreateTable(
                name: "BatchAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProductBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionById = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValue = table.Column<int>(type: "integer", nullable: false),
                    NewValue = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchAudits_ProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchAudits_Staffs_ActionById",
                        column: x => x.ActionById,
                        principalTable: "Staffs",
                        principalColumn: "Id");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Price_Min",
                table: "Products",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Quantity_Min",
                table: "Products",
                sql: "\"Quantity\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatches_Code",
                table: "ProductBatches",
                column: "Code",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductBatch_Quantity_Min",
                table: "ProductBatches",
                sql: "\"Quantity\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAudits_ActionById",
                table: "BatchAudits",
                column: "ActionById");

            migrationBuilder.CreateIndex(
                name: "IX_BatchAudits_ProductBatchId",
                table: "BatchAudits",
                column: "ProductBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchAudits");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Price_Min",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Quantity_Min",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductBatches_Code",
                table: "ProductBatches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductBatch_Quantity_Min",
                table: "ProductBatches");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ProductBatches");

            migrationBuilder.DropColumn(
                name: "IsExpired",
                table: "ProductBatches");

            migrationBuilder.DropSequence(
                name: "BatchCodeSeq");

            migrationBuilder.CreateSequence<int>(
                name: "ProductCodeSeq");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValueSql: "'SP' || LPAD(nextval('\"ProductCodeSeq\"')::text, 6, '0')",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "ProductBatches",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'LOT' || LPAD(nextval('\"BatchCodeSeq\"')::text, 6, '0')");

            migrationBuilder.CreateTable(
                name: "InternalConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ConversationName = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: true, defaultValueSql: "true"),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalConversations", x => x.Id);
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
                    InternalConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false)
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
        }
    }
}
