using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OmniChat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropBatchSequenceCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"ProductBatches\" ALTER COLUMN \"Code\" DROP DEFAULT;");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"BatchCodeSeq\" CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SEQUENCE IF NOT EXISTS \"BatchCodeSeq\" START WITH 1 INCREMENT BY 1;");

            migrationBuilder.Sql("ALTER TABLE \"ProductBatches\" ALTER COLUMN \"Code\" SET DEFAULT ('LOT' || LPAD(nextval('\"BatchCodeSeq\"')::text, 6, '0'));");
        }
    }
}
