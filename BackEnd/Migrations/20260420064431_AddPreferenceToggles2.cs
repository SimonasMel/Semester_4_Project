using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferenceToggles2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseBodyType",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseBrand",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseEnginePower",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseFuelType",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseMileage",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsePrice",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseTransmission",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseYear",
                table: "UserPreferences",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseBodyType",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UseBrand",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UseEnginePower",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UseFuelType",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UseMileage",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UsePrice",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UseTransmission",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UseYear",
                table: "UserPreferences");
        }
    }
}
