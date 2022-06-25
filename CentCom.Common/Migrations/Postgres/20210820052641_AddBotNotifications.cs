using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddBotNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notified_failures",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                check_history_id = table.Column<long>(type: "bigint", nullable: false),
                timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_notified_failures", x => x.id);
                table.ForeignKey(
                    name: "fk_notified_failures_check_history_check_history_id",
                    column: x => x.check_history_id,
                    principalTable: "check_history",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_notified_failures_check_history_id",
            table: "notified_failures",
            column: "check_history_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "notified_failures");
    }
}