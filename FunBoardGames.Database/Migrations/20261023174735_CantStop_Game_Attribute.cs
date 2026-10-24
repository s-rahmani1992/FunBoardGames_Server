using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class CantStop_Game_Attribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "marker_cone_count",
                table: "CantStop_Games",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "round_time",
                table: "CantStop_Games",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "timeout_limit",
                table: "CantStop_Games",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "win_cone_count",
                table: "CantStop_Games",
                type: "integer",
                nullable: false,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "marker_cone_count",
                table: "CantStop_Games");

            migrationBuilder.DropColumn(
                name: "round_time",
                table: "CantStop_Games");

            migrationBuilder.DropColumn(
                name: "timeout_limit",
                table: "CantStop_Games");

            migrationBuilder.DropColumn(
                name: "win_cone_count",
                table: "CantStop_Games");
        }
    }
}
