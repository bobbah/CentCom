using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddFlatFileVersioning : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "flat_bans_version",
            columns: table => new
            {
                id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                name = table.Column<string>(nullable: false),
                version = table.Column<long>(nullable: false),
                performed_at = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_flat_bans_version", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_flat_bans_version_name_version",
            table: "flat_bans_version",
            columns: new[] { "name", "version" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "flat_bans_version");
    }
}