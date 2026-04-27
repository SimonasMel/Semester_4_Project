using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class AddCarLikesAndMutualMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarLikes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LikerUserId = table.Column<string>(type: "text", nullable: false),
                    LikedCarId = table.Column<string>(type: "text", nullable: false),
                    LikedCarOwnerId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarLikes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MutualMatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CurrentUserId = table.Column<string>(type: "text", nullable: false),
                    MatchedUserId = table.Column<string>(type: "text", nullable: false),
                    CurrentUserCarId = table.Column<string>(type: "text", nullable: false),
                    MatchedUserCarId = table.Column<string>(type: "text", nullable: false),
                    MatchedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutualMatches", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarLikes_LikerUserId_LikedCarId",
                table: "CarLikes",
                columns: new[] { "LikerUserId", "LikedCarId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MutualMatches_CurrentUserId_MatchedUserId_CurrentUserCarId_~",
                table: "MutualMatches",
                columns: new[] { "CurrentUserId", "MatchedUserId", "CurrentUserCarId", "MatchedUserCarId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarLikes");

            migrationBuilder.DropTable(
                name: "MutualMatches");
        }
    }
}
