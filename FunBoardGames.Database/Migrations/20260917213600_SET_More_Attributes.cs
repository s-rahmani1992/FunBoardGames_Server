using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class SET_More_Attributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "hint_limit",
                table: "SET_Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "round_time",
                table: "SET_Games",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "wrong_limit",
                table: "SET_Games",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_hint_limit",
                table: "SET_Games",
                sql: "hint_limit >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_round_time",
                table: "SET_Games",
                sql: "round_time > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_wrong_limit",
                table: "SET_Games",
                sql: "wrong_limit > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_hint_limit",
                table: "SET_Games");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_round_time",
                table: "SET_Games");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_wrong_limit",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "hint_limit",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "round_time",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "wrong_limit",
                table: "SET_Games");
        }
    }
}
