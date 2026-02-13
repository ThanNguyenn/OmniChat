using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderItemAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValueSql: "false");

            migrationBuilder.Sql(
      @"ALTER TABLE ""OrderItems"" 
          ALTER COLUMN ""Quantity"" 
          TYPE integer 
          USING ""Quantity""::integer;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orders");

            migrationBuilder.Sql(
         @"ALTER TABLE ""OrderItems"" 
          ALTER COLUMN ""Quantity"" 
          TYPE text;");
        }
    }
}
