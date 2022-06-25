using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CentCom.Common.Migrations.Postgres;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BanSources",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                Name = table.Column<string>(nullable: true),
                Display = table.Column<string>(nullable: true),
                RoleplayLevel = table.Column<long>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BanSources", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Bans",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                Source = table.Column<int>(nullable: false),
                BanType = table.Column<long>(nullable: false),
                CKey = table.Column<string>(maxLength: 32, nullable: false),
                BannedOn = table.Column<DateTime>(nullable: false),
                BannedBy = table.Column<string>(maxLength: 32, nullable: false),
                Reason = table.Column<string>(nullable: true),
                Expires = table.Column<DateTime>(nullable: true),
                UnbannedBy = table.Column<string>(maxLength: 32, nullable: true),
                IP = table.Column<IPAddress>(nullable: true),
                CID = table.Column<long>(nullable: true),
                BanID = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bans", x => x.Id);
                table.ForeignKey(
                    name: "FK_Bans_BanSources_Source",
                    column: x => x.Source,
                    principalTable: "BanSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "JobBans",
            columns: table => new
            {
                BanId = table.Column<int>(nullable: false),
                Job = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JobBans", x => new { x.BanId, x.Job });
                table.ForeignKey(
                    name: "FK_JobBans_Bans_BanId",
                    column: x => x.BanId,
                    principalTable: "Bans",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Bans_CKey",
            table: "Bans",
            column: "CKey");

        migrationBuilder.CreateIndex(
            name: "IX_Bans_Source",
            table: "Bans",
            column: "Source");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "JobBans");

        migrationBuilder.DropTable(
            name: "Bans");

        migrationBuilder.DropTable(
            name: "BanSources");
    }
}