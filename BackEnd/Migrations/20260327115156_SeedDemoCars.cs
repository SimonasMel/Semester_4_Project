using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "Brand", "Model", "ProductionYear", "FuelType", "Transmission", "BodyType", "EnginePowerKW", "FuelConsumptionLitersPer100Km", "EnergyConsumptionKWhPer100Km", "Price", "MileageKm", "PrimaryImagePath", "Description", "Location", "ContactInfo", "VIN", "Features", "AdditionalImagePaths" },
                values: new object[,]
                {
                    {
                        "porsche-911-demo-001",
                        "Porsche",
                        "911",
                        2023,
                        0, // Petrol
                        1, // Automatic
                        4, // Coupe
                        331,
                        10.2,
                        null,
                        135000,
                        8000,
                        "https://images.unsplash.com/photo-1503376780353-7e6692767b70",
                        "Low mileage, weekend driver Porsche.",
                        "Vilnius",
                        "porsche@dealer.lt",
                        "WP0ZZZ99ZTS392124",
                        "[\"Leather seats\",\"Sport exhaust\",\"Bose audio\"]",
                        "[]"
                    },
                    {
                        "tesla-models-demo-002",
                        "Tesla",
                        "Model S",
                        2022,
                        2, // Electric
                        1, // Automatic
                        0, // Sedan
                        500,
                        null,
                        19.0,
                        98000,
                        25000,
                        "https://images.unsplash.com/photo-1560958089-b8a1929cea89",
                        "Ludicrous mode enabled electric rocket.",
                        "Kaunas",
                        "tesla@dealer.lt",
                        "5YJSA1E26MF168294",
                        "[\"Autopilot\",\"Panoramic roof\",\"Premium audio\"]",
                        "[]"
                    },
                    {
                        "ford-mustang-demo-003",
                        "Ford",
                        "Mustang",
                        1969,
                        0, // Petrol
                        0, // Manual
                        4, // Coupe
                        220,
                        15.0,
                        null,
                        65000,
                        120000,
                        "https://images.unsplash.com/photo-1584345604476-8ec5e12e42dd",
                        "Classic muscle car. Sounds like thunder.",
                        "Klaipėda",
                        "mustang@classics.lt",
                        "1FAFP404X1F192837",
                        "[\"V8 engine\",\"Classic interior\",\"Chrome wheels\"]",
                        "[]"
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: "porsche-911-demo-001");

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: "tesla-models-demo-002");

            migrationBuilder.DeleteData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: "ford-mustang-demo-003");
        }
    }
}
