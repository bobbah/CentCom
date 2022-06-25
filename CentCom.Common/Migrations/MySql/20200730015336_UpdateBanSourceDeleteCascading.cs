using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MySql;

public partial class UpdateBanSourceDeleteCascading : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Bans_BanSources_Source",
            table: "Bans");

        migrationBuilder.AddForeignKey(
            name: "FK_Bans_BanSources_Source",
            table: "Bans",
            column: "Source",
            principalTable: "BanSources",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Bans_BanSources_Source",
            table: "Bans");

        migrationBuilder.AddForeignKey(
            name: "FK_Bans_BanSources_Source",
            table: "Bans",
            column: "Source",
            principalTable: "BanSources",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}