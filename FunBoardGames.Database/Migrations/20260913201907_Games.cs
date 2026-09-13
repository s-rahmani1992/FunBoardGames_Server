using System;
using FunBoardGames.Network.SignalR.Shared;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class Games : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:game_type", "set,cant_stop");

            migrationBuilder.CreateTable(
                name: "Board_Games",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    game_type = table.Column<BoardGameType>(type: "game_type", nullable: false, defaultValue: BoardGameType.SET),
                    name = table.Column<string>(type: "text", nullable: false, defaultValue: "game"),
                    player_count = table.Column<long>(type: "bigint", nullable: false, defaultValue: 2L),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Board_Games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CantStop_Games",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    board_data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CantStop_Games", x => x.id);
                    table.ForeignKey(
                        name: "FK_CantStop_Games_Board_Games_id",
                        column: x => x.id,
                        principalTable: "Board_Games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SET_Games",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    visible_card_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 12)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SET_Games", x => x.id);
                    table.ForeignKey(
                        name: "FK_SET_Games_Board_Games_id",
                        column: x => x.id,
                        principalTable: "Board_Games",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CantStop_Games");

            migrationBuilder.DropTable(
                name: "SET_Games");

            migrationBuilder.DropTable(
                name: "Board_Games");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:game_type", "set,cant_stop");
        }
    }
}
