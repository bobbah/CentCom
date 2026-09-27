using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddBanSourceLookupIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_bans_source_ban_id",
            table: "bans",
            columns: new[] { "source", "ban_id" });

        migrationBuilder.CreateIndex(
            name: "ix_bans_source_banned_on",
            table: "bans",
            columns: new[] { "source", "banned_on" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "ix_bans_source_ban_id", table: "bans");
        migrationBuilder.DropIndex(name: "ix_bans_source_banned_on", table: "bans");
    }
}
