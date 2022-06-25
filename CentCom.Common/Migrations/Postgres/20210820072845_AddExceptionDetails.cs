using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddExceptionDetails : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "exception_detailed",
            table: "check_history",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "exception_detailed",
            table: "check_history");
    }
}