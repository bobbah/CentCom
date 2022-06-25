using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.MariaDb;

public partial class AddBotNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                name: "NotifiedFailures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CheckHistoryId = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifiedFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotifiedFailures_CheckHistory_CheckHistoryId",
                        column: x => x.CheckHistoryId,
                        principalTable: "CheckHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_NotifiedFailures_CheckHistoryId",
            table: "NotifiedFailures",
            column: "CheckHistoryId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "NotifiedFailures");
    }
}