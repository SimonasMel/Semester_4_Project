using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDemoColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "Cars",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Mark the seeded demo cars
            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: "porsche-911-demo-001",
                column: "IsDemo",
                value: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: "tesla-models-demo-002",
                column: "IsDemo",
                value: true);

            migrationBuilder.UpdateData(
                table: "Cars",
                keyColumn: "Id",
                keyValue: "ford-mustang-demo-003",
                column: "IsDemo",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "Cars");
        }
    }
}
