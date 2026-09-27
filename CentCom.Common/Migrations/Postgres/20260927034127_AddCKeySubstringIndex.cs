using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentCom.Common.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddCKeySubstringIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm");
            migrationBuilder.Sql(
                "CREATE INDEX ix_bans_lower_c_key_trgm ON bans USING gin (lower(c_key) gin_trgm_ops)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX ix_bans_lower_c_key_trgm");
        }
    }
}
