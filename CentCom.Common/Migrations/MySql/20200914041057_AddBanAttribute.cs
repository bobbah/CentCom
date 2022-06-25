using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MySql;

public partial class AddBanAttribute : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "BanAttributes",
            table: "Bans",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "BanAttributes",
            table: "Bans");
    }
}