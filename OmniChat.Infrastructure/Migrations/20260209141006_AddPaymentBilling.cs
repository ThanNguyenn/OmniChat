using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Orders"
                ALTER COLUMN "OrderDate"
                TYPE timestamp with time zone
                USING "OrderDate"::timestamp with time zone;
            """);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<double>(type: "double precision", nullable: false),
                    PayStatus = table.Column<string>(type: "text", nullable: false),
                    PayMethod = table.Column<string>(type: "text", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_OrderId",
                table: "BillingItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingItems_PaymentId",
                table: "BillingItems",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "OrderDate",
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
