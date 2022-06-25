using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MariaDb;

public partial class AddExceptionDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
                name: "ExceptionDetailed",
                table: "CheckHistory",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ExceptionDetailed",
            table: "CheckHistory");
    }
}