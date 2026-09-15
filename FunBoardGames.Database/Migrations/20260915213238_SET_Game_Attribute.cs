using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class SET_Game_Attribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "color_attribute",
                table: "SET_Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "count_attribute",
                table: "SET_Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "shading_attribute",
                table: "SET_Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "shape_attribute",
                table: "SET_Games",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_color_attribute",
                table: "SET_Games",
                sql: "color_attribute BETWEEN 0 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_count_attribute",
                table: "SET_Games",
                sql: "count_attribute BETWEEN 0 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_shading_attribute",
                table: "SET_Games",
                sql: "shading_attribute BETWEEN 0 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SET_Games_shape_attribute",
                table: "SET_Games",
                sql: "shape_attribute BETWEEN 0 AND 2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_color_attribute",
                table: "SET_Games");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_count_attribute",
                table: "SET_Games");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_shading_attribute",
                table: "SET_Games");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SET_Games_shape_attribute",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "color_attribute",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "count_attribute",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "shading_attribute",
                table: "SET_Games");

            migrationBuilder.DropColumn(
                name: "shape_attribute",
                table: "SET_Games");
        }
    }
}
