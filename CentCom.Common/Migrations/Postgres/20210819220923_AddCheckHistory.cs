using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddCheckHistory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "check_history",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                parser = table.Column<string>(type: "text", nullable: false),
                started = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_data_fetch = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                completed_upload = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                failed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                added = table.Column<int>(type: "integer", nullable: false),
                updated = table.Column<int>(type: "integer", nullable: false),
                deleted = table.Column<int>(type: "integer", nullable: false),
                erroneous = table.Column<int>(type: "integer", nullable: false),
                success = table.Column<bool>(type: "boolean", nullable: false),
                complete_refresh = table.Column<bool>(type: "boolean", nullable: false),
                exception = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_check_history", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_check_history_parser_started",
            table: "check_history",
            columns: new[] { "parser", "started" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "check_history");
    }
}