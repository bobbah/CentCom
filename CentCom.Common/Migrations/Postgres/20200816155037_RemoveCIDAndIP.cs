using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.Postgres;

public partial class RemoveCIDAndIP : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "cid",
            table: "bans");

        migrationBuilder.DropColumn(
            name: "ip",
            table: "bans");

        migrationBuilder.Sql("UPDATE ban_sources SET roleplay_level = 2 WHERE name = 'fulp'");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "cid",
            table: "bans",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<IPAddress>(
            name: "ip",
            table: "bans",
            type: "inet",
            nullable: true);

        migrationBuilder.Sql("UPDATE ban_sources SET roleplay_level = 1 WHERE name = 'fulp'");
    }
}