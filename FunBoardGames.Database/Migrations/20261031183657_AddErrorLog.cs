using System;
using FunBoardGames.Domain.ErrorHandling;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddErrorLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:error_type", "exception,invalid_state")
                .Annotation("Npgsql:Enum:game_type", "set,cant_stop")
                .OldAnnotation("Npgsql:Enum:game_type", "set,cant_stop");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "SET_Games",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "CantStop_Games",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "player_count",
                table: "Board_Games",
                type: "integer",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 2L);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Board_Games",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "Error_Logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<ErrorType>(type: "error_type", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    message_name = table.Column<string>(type: "text", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Error_Logs", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Error_Logs");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:game_type", "set,cant_stop")
                .OldAnnotation("Npgsql:Enum:error_type", "exception,invalid_state")
                .OldAnnotation("Npgsql:Enum:game_type", "set,cant_stop");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "SET_Games",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "CantStop_Games",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "player_count",
                table: "Board_Games",
                type: "bigint",
                nullable: false,
                defaultValue: 2L,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 2);

            migrationBuilder.AlterColumn<long>(
                name: "id",
                table: "Board_Games",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
