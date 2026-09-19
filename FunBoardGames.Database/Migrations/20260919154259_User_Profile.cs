using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class User_Profile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "User-Profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    player_name = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User-Profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_User-Profiles_User_Credentials_user_id",
                        column: x => x.user_id,
                        principalTable: "User_Credentials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User-Profiles_user_id",
                table: "User-Profiles",
                column: "user_id",
                unique: true);

            // The player name moves out of the credentials row and into a profile of its own, so
            // every existing user keeps the name they signed up with. This has to run while
            // User_Credentials.name still exists, hence before the DropColumn below. The avatar
            // column is left to its database default.
            migrationBuilder.Sql(
                """
                INSERT INTO "User-Profiles" ("user_id", "player_name")
                SELECT "id", "name" FROM "User_Credentials";
                """);

            migrationBuilder.DropColumn(
                name: "name",
                table: "User_Credentials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "User_Credentials",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Move the names back before the profiles they live in are dropped.
            migrationBuilder.Sql(
                """
                UPDATE "User_Credentials" AS c
                SET "name" = p."player_name"
                FROM "User-Profiles" AS p
                WHERE p."user_id" = c."id";
                """);

            migrationBuilder.DropTable(
                name: "User-Profiles");
        }
    }
}
