using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProductionYear = table.Column<int>(type: "INTEGER", nullable: false),
                    FuelType = table.Column<int>(type: "INTEGER", nullable: false),
                    Transmission = table.Column<int>(type: "INTEGER", nullable: false),
                    BodyType = table.Column<int>(type: "INTEGER", nullable: false),
                    EnginePowerKW = table.Column<int>(type: "INTEGER", nullable: false),
                    FuelConsumptionLitersPer100Km = table.Column<double>(type: "REAL", nullable: true),
                    EnergyConsumptionKWhPer100Km = table.Column<double>(type: "REAL", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    MileageKm = table.Column<int>(type: "INTEGER", nullable: false),
                    PrimaryImagePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AdditionalImagePaths = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Features = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ContactInfo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    VIN = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
