using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardGamesTpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameType = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PlayerCount = table.Column<long>(type: "bigint", nullable: false),
                    Created_At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Deleted_At = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CantStopGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    BoardData = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CantStopGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CantStopGames_Games_Id",
                        column: x => x.Id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SETGames",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    GuessTime = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SETGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SETGames_Games_Id",
                        column: x => x.Id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CantStopGames");

            migrationBuilder.DropTable(
                name: "SETGames");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
