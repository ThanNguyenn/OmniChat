using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRelationShipBetweenClaimKeywordType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_KeywordTypes_KeywordTypeId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_KeywordTypeId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "KeywordTypeId",
                table: "Claims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KeywordTypeId",
                table: "Claims",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Claims_KeywordTypeId",
                table: "Claims",
                column: "KeywordTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_KeywordTypes_KeywordTypeId",
                table: "Claims",
                column: "KeywordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
