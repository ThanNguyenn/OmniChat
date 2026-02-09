using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StaffKeywordType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KeyWordTypeId",
                table: "Staffs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_KeyWordTypeId",
                table: "Staffs",
                column: "KeyWordTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Staffs_KeywordTypes_KeyWordTypeId",
                table: "Staffs",
                column: "KeyWordTypeId",
                principalTable: "KeywordTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staffs_KeywordTypes_KeyWordTypeId",
                table: "Staffs");

            migrationBuilder.DropIndex(
                name: "IX_Staffs_KeyWordTypeId",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "KeyWordTypeId",
                table: "Staffs");
        }
    }
}
