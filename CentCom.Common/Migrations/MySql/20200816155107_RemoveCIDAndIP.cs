using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MySql;

public partial class RemoveCIDAndIP : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CID",
            table: "Bans");

        migrationBuilder.DropColumn(
            name: "IP",
            table: "Bans");

        migrationBuilder.Sql("UPDATE BanSources SET RoleplayLevel = 2 WHERE Name = 'fulp'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "CID",
            table: "Bans",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "IP",
            table: "Bans",
            type: "longtext CHARACTER SET utf8mb4",
            nullable: true);

        migrationBuilder.Sql("UPDATE BanSources SET RoleplayLevel = 1 WHERE Name = 'fulp'");
    }
}