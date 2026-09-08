using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunBoardGames.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameAuthTokenHashAndAddUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "UserCredentials",
                newName: "AuthTokenHash");

            // A partial unique index, not a plain one: two rows may share a
            // (Name, DeviceId) pair as long as at most one of them is active
            // (DeletedAt IS NULL), so a soft-deleted account doesn't block
            // someone signing up again with the same name on the same device.
            // Created via raw SQL (rather than modelBuilder.HasIndex/HasFilter)
            // so EF Core's InMemory provider - used by the unit tests - never
            // sees or tries to enforce it; see FunBoardGamesDbContext for the
            // full rationale.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_UserCredentials_Name_DeviceId_Active\" " +
                "ON \"UserCredentials\" (\"Name\", \"DeviceId\") " +
                "WHERE \"DeletedAt\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_UserCredentials_Name_DeviceId_Active\";");

            migrationBuilder.RenameColumn(
                name: "AuthTokenHash",
                table: "UserCredentials",
                newName: "Password");
        }
    }
}
