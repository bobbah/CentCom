using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CentCom.Common.Migrations.MySql
{
    /// <inheritdoc />
    public partial class AddCKeySubstringIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanCKeyGrams",
                columns: table => new
                {
                    BanId = table.Column<int>(type: "int", nullable: false),
                    Gram = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false, collation: "ascii_bin")
                        .Annotation("MySql:CharSet", "ascii")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanCKeyGrams", x => new { x.Gram, x.BanId });
                    table.ForeignKey(
                        name: "FK_BanCKeyGrams_Bans_BanId",
                        column: x => x.BanId,
                        principalTable: "Bans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BanCKeyGrams_BanId",
                table: "BanCKeyGrams",
                column: "BanId");

            migrationBuilder.Sql("""
                CREATE TRIGGER `trg_bans_ckey_grams_insert`
                AFTER INSERT ON `Bans` FOR EACH ROW
                BEGIN
                    DECLARE pos INT DEFAULT 1;
                    WHILE pos <= CHAR_LENGTH(NEW.`CKey`) - 2 DO
                        IF NOT EXISTS (SELECT 1 FROM `BanCKeyGrams`
                                       WHERE `Gram` = SUBSTRING(LOWER(NEW.`CKey`), pos, 3)
                                         AND `BanId` = NEW.`Id`) THEN
                            INSERT INTO `BanCKeyGrams` (`Gram`, `BanId`)
                            VALUES (SUBSTRING(LOWER(NEW.`CKey`), pos, 3), NEW.`Id`);
                        END IF;
                        SET pos = pos + 1;
                    END WHILE;
                END
                """);

            migrationBuilder.Sql("""
                CREATE TRIGGER `trg_bans_ckey_grams_update`
                AFTER UPDATE ON `Bans` FOR EACH ROW
                BEGIN
                    DECLARE pos INT DEFAULT 1;
                    IF BINARY NEW.`CKey` <> BINARY OLD.`CKey` THEN
                        DELETE FROM `BanCKeyGrams` WHERE `BanId` = NEW.`Id`;
                        WHILE pos <= CHAR_LENGTH(NEW.`CKey`) - 2 DO
                            IF NOT EXISTS (SELECT 1 FROM `BanCKeyGrams`
                                           WHERE `Gram` = SUBSTRING(LOWER(NEW.`CKey`), pos, 3)
                                             AND `BanId` = NEW.`Id`) THEN
                                INSERT INTO `BanCKeyGrams` (`Gram`, `BanId`)
                                VALUES (SUBSTRING(LOWER(NEW.`CKey`), pos, 3), NEW.`Id`);
                            END IF;
                            SET pos = pos + 1;
                        END WHILE;
                    END IF;
                END
                """);

            migrationBuilder.Sql("""
                INSERT INTO `BanCKeyGrams` (`BanId`, `Gram`)
                SELECT DISTINCT b.`Id`, SUBSTRING(LOWER(b.`CKey`), t.n * 10 + u.n + 1, 3)
                FROM `Bans` AS b
                CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2) AS t
                CROSS JOIN (SELECT 0 AS n UNION ALL SELECT 1 UNION ALL SELECT 2
                            UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5
                            UNION ALL SELECT 6 UNION ALL SELECT 7 UNION ALL SELECT 8
                            UNION ALL SELECT 9) AS u
                WHERE t.n * 10 + u.n + 1 <= CHAR_LENGTH(b.`CKey`) - 2
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER `trg_bans_ckey_grams_update`");
            migrationBuilder.Sql("DROP TRIGGER `trg_bans_ckey_grams_insert`");
            migrationBuilder.DropTable(name: "BanCKeyGrams");
        }
    }
}
