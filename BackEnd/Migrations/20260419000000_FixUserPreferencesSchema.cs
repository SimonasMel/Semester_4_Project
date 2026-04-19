using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class FixUserPreferencesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing UserPreferences table with incorrect schema
            migrationBuilder.DropTable(
                name: "UserPreferences");

            // Create UserPreferences table with correct column names
            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PreferredBrand = table.Column<string>(type: "TEXT", nullable: true),
                    PreferredYear = table.Column<int>(type: "INTEGER", nullable: true),
                    PreferredPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    MileageKm = table.Column<int>(type: "INTEGER", nullable: true),
                    EnginePowerKW = table.Column<int>(type: "INTEGER", nullable: true),
                    FuelType = table.Column<int>(type: "INTEGER", nullable: true),
                    Transmission = table.Column<int>(type: "INTEGER", nullable: true),
                    BodyType = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PreferredBrand = table.Column<string>(type: "TEXT", nullable: true),
                    MinPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    MinYear = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxYear = table.Column<int>(type: "INTEGER", nullable: true),
                    MaxMileageKm = table.Column<int>(type: "INTEGER", nullable: true),
                    MinEnginePowerKW = table.Column<int>(type: "INTEGER", nullable: true),
                    FuelType = table.Column<int>(type: "INTEGER", nullable: true),
                    Transmission = table.Column<int>(type: "INTEGER", nullable: true),
                    BodyType = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);
        }
    }
}
