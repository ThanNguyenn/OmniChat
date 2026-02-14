using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDatetimeToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        ALTER TABLE ""InstagramOathTokens""
        ALTER COLUMN ""AccessTokenExpiredDate""
        TYPE timestamp with time zone
        USING ""AccessTokenExpiredDate""::timestamp with time zone;
              ");

            migrationBuilder.Sql(@"
        ALTER TABLE ""FacebookOathTokens""
        ALTER COLUMN ""AccessTokenExpiredDate""
        TYPE timestamp with time zone
        USING ""AccessTokenExpiredDate""::timestamp with time zone;
             ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccessTokenExpiredDate",
                table: "InstagramOathTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "AccessTokenExpiredDate",
                table: "FacebookOathTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
