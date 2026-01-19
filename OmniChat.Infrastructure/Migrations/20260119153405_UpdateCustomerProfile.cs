using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "CustomerProfiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CustomerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Staffs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "Staffs");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "CustomerProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CustomerProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Gender",
                table: "CustomerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CustomerProfiles",
                type: "text",
                nullable: true);
        }
    }
}
