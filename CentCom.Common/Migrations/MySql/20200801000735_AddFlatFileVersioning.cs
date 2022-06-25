using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MySql;

public partial class AddFlatFileVersioning : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "FlatBansVersion",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(nullable: false),
                Version = table.Column<uint>(nullable: false),
                PerformedAt = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_FlatBansVersion", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_FlatBansVersion_Name_Version",
            table: "FlatBansVersion",
            columns: new[] { "Name", "Version" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "FlatBansVersion");
    }
}