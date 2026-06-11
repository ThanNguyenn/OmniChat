using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkTransactionAndAllocationOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "Allocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Allocations_TransactionId",
                table: "Allocations",
                column: "TransactionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Allocations_Transactions_TransactionId",
                table: "Allocations",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Allocations_Transactions_TransactionId",
                table: "Allocations");

            migrationBuilder.DropIndex(
                name: "IX_Allocations_TransactionId",
                table: "Allocations");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Allocations");
        }
    }
}
