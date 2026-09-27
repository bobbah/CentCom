using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MySql;

public partial class AddBanSourceLookupIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
                name: "IX_Bans_Source_BanID",
                table: "Bans",
                columns: new[] { "Source", "BanID" })
            .Annotation("MySql:IndexPrefixLength", new[] { 0, 128 });

        migrationBuilder.CreateIndex(
            name: "IX_Bans_Source_BannedOn",
            table: "Bans",
            columns: new[] { "Source", "BannedOn" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Bans_Source_BanID", table: "Bans");
        migrationBuilder.DropIndex(name: "IX_Bans_Source_BannedOn", table: "Bans");
    }
}
