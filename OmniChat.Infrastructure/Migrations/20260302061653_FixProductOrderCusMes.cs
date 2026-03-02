using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductOrderCusMes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "OrderCodeSeq");

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
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValueSql: "'OD' || LPAD(nextval('\"OrderCodeSeq\"')::text, 6, '0')",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "CustomerMessages",
                type: "boolean",
                nullable: false,
                defaultValueSql: "false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
         name: "IsRead",
         table: "CustomerMessages");

            // Remove default
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
                table: "Orders",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValueSql: "'OD' || LPAD(nextval('\"OrderCodeSeq\"')::text, 6, '0')");

            // drop sequence
            migrationBuilder.DropSequence(name: "OrderCodeSeq");
            migrationBuilder.DropSequence(name: "ProductCodeSeq");
        }
    }
}
