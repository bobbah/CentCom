using Microsoft.EntityFrameworkCore.Migrations;

namespace CentCom.Common.Migrations.Postgres;

public partial class AddSnakeCaseNamingConvention : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Bans_BanSources_Source",
            table: "Bans");

        migrationBuilder.DropForeignKey(
            name: "FK_JobBans_Bans_BanId",
            table: "JobBans");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Bans",
            table: "Bans");

        migrationBuilder.DropPrimaryKey(
            name: "PK_JobBans",
            table: "JobBans");

        migrationBuilder.DropPrimaryKey(
            name: "PK_BanSources",
            table: "BanSources");

        migrationBuilder.RenameTable(
            name: "Bans",
            newName: "bans");

        migrationBuilder.RenameTable(
            name: "JobBans",
            newName: "job_bans");

        migrationBuilder.RenameTable(
            name: "BanSources",
            newName: "ban_sources");

        migrationBuilder.RenameColumn(
            name: "Source",
            table: "bans",
            newName: "source");

        migrationBuilder.RenameColumn(
            name: "Reason",
            table: "bans",
            newName: "reason");

        migrationBuilder.RenameColumn(
            name: "IP",
            table: "bans",
            newName: "ip");

        migrationBuilder.RenameColumn(
            name: "Expires",
            table: "bans",
            newName: "expires");

        migrationBuilder.RenameColumn(
            name: "CID",
            table: "bans",
            newName: "cid");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "bans",
            newName: "id");

        migrationBuilder.RenameColumn(
            name: "UnbannedBy",
            table: "bans",
            newName: "unbanned_by");

        migrationBuilder.RenameColumn(
            name: "CKey",
            table: "bans",
            newName: "c_key");

        migrationBuilder.RenameColumn(
            name: "BannedOn",
            table: "bans",
            newName: "banned_on");

        migrationBuilder.RenameColumn(
            name: "BannedBy",
            table: "bans",
            newName: "banned_by");

        migrationBuilder.RenameColumn(
            name: "BanType",
            table: "bans",
            newName: "ban_type");

        migrationBuilder.RenameColumn(
            name: "BanID",
            table: "bans",
            newName: "ban_id");

        migrationBuilder.RenameIndex(
            name: "IX_Bans_Source",
            table: "bans",
            newName: "ix_bans_source");

        migrationBuilder.RenameIndex(
            name: "IX_Bans_CKey",
            table: "bans",
            newName: "ix_bans_c_key");

        migrationBuilder.RenameColumn(
            name: "Job",
            table: "job_bans",
            newName: "job");

        migrationBuilder.RenameColumn(
            name: "BanId",
            table: "job_bans",
            newName: "ban_id");

        migrationBuilder.RenameColumn(
            name: "Name",
            table: "ban_sources",
            newName: "name");

        migrationBuilder.RenameColumn(
            name: "Display",
            table: "ban_sources",
            newName: "display");

        migrationBuilder.RenameColumn(
            name: "Id",
            table: "ban_sources",
            newName: "id");

        migrationBuilder.RenameColumn(
            name: "RoleplayLevel",
            table: "ban_sources",
            newName: "roleplay_level");

        migrationBuilder.AddPrimaryKey(
            name: "pk_bans",
            table: "bans",
            column: "id");

        migrationBuilder.AddPrimaryKey(
            name: "pk_job_bans",
            table: "job_bans",
            columns: new[] { "ban_id", "job" });

        migrationBuilder.AddPrimaryKey(
            name: "pk_ban_sources",
            table: "ban_sources",
            column: "id");

        migrationBuilder.AddForeignKey(
            name: "fk_bans_ban_sources_source_navigation_id",
            table: "bans",
            column: "source",
            principalTable: "ban_sources",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_job_bans_bans_ban_id",
            table: "job_bans",
            column: "ban_id",
            principalTable: "bans",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "fk_bans_ban_sources_source_navigation_id",
            table: "bans");

        migrationBuilder.DropForeignKey(
            name: "fk_job_bans_bans_ban_id",
            table: "job_bans");

        migrationBuilder.DropPrimaryKey(
            name: "pk_bans",
            table: "bans");

        migrationBuilder.DropPrimaryKey(
            name: "pk_job_bans",
            table: "job_bans");

        migrationBuilder.DropPrimaryKey(
            name: "pk_ban_sources",
            table: "ban_sources");

        migrationBuilder.RenameTable(
            name: "bans",
            newName: "Bans");

        migrationBuilder.RenameTable(
            name: "job_bans",
            newName: "JobBans");

        migrationBuilder.RenameTable(
            name: "ban_sources",
            newName: "BanSources");

        migrationBuilder.RenameColumn(
            name: "source",
            table: "Bans",
            newName: "Source");

        migrationBuilder.RenameColumn(
            name: "reason",
            table: "Bans",
            newName: "Reason");

        migrationBuilder.RenameColumn(
            name: "ip",
            table: "Bans",
            newName: "IP");

        migrationBuilder.RenameColumn(
            name: "expires",
            table: "Bans",
            newName: "Expires");

        migrationBuilder.RenameColumn(
            name: "cid",
            table: "Bans",
            newName: "CID");

        migrationBuilder.RenameColumn(
            name: "id",
            table: "Bans",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "unbanned_by",
            table: "Bans",
            newName: "UnbannedBy");

        migrationBuilder.RenameColumn(
            name: "c_key",
            table: "Bans",
            newName: "CKey");

        migrationBuilder.RenameColumn(
            name: "banned_on",
            table: "Bans",
            newName: "BannedOn");

        migrationBuilder.RenameColumn(
            name: "banned_by",
            table: "Bans",
            newName: "BannedBy");

        migrationBuilder.RenameColumn(
            name: "ban_type",
            table: "Bans",
            newName: "BanType");

        migrationBuilder.RenameColumn(
            name: "ban_id",
            table: "Bans",
            newName: "BanID");

        migrationBuilder.RenameIndex(
            name: "ix_bans_source",
            table: "Bans",
            newName: "IX_Bans_Source");

        migrationBuilder.RenameIndex(
            name: "ix_bans_c_key",
            table: "Bans",
            newName: "IX_Bans_CKey");

        migrationBuilder.RenameColumn(
            name: "job",
            table: "JobBans",
            newName: "Job");

        migrationBuilder.RenameColumn(
            name: "ban_id",
            table: "JobBans",
            newName: "BanId");

        migrationBuilder.RenameColumn(
            name: "name",
            table: "BanSources",
            newName: "Name");

        migrationBuilder.RenameColumn(
            name: "display",
            table: "BanSources",
            newName: "Display");

        migrationBuilder.RenameColumn(
            name: "id",
            table: "BanSources",
            newName: "Id");

        migrationBuilder.RenameColumn(
            name: "roleplay_level",
            table: "BanSources",
            newName: "RoleplayLevel");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Bans",
            table: "Bans",
            column: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_JobBans",
            table: "JobBans",
            columns: new[] { "BanId", "Job" });

        migrationBuilder.AddPrimaryKey(
            name: "PK_BanSources",
            table: "BanSources",
            column: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Bans_BanSources_Source",
            table: "Bans",
            column: "Source",
            principalTable: "BanSources",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_JobBans_Bans_BanId",
            table: "JobBans",
            column: "BanId",
            principalTable: "Bans",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}