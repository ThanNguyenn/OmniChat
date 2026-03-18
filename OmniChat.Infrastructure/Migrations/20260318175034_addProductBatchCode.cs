using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addProductBatchCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                    migrationBuilder.AddColumn<string>(
                        name: "Code",
                        table: "ProductBatches",
                        type: "text",
                        nullable: false,
                        defaultValue: "");

                    migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION generate_product_batch_code()
                RETURNS trigger AS $$
                BEGIN
                    NEW.""Code"" := 'LOT' || TO_CHAR(NEW.""ExpiryDate"", 'YYYYMMDD');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

                    migrationBuilder.Sql(@"
                CREATE TRIGGER trg_generate_product_batch_code
                BEFORE INSERT
                ON ""ProductBatches""
                FOR EACH ROW
                EXECUTE FUNCTION generate_product_batch_code();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_generate_product_batch_code ON ""ProductBatches"";");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS generate_product_batch_code();");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductBatches");
        }
    }
}
