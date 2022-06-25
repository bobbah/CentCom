using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddBanAttribute : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ban_attributes",
            table: "bans",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ban_attributes",
            table: "bans");
    }
}