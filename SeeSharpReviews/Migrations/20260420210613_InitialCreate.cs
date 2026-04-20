using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SeeSharpReviews.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SSR_ROLES",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RoleName = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSR_ROLES", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SSR_USERS",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Username = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    RoleId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSR_USERS", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_SSR_USERS_SSR_ROLES_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SSR_ROLES",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SSR_REVIEWS",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UserId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    MovieApiId = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    MovieTitle = table.Column<string>(type: "NVARCHAR2(255)", maxLength: 255, nullable: false),
                    MoviePosterPath = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    ReviewText = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    Rating = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SSR_REVIEWS", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_SSR_REVIEWS_SSR_USERS_UserId",
                        column: x => x.UserId,
                        principalTable: "SSR_USERS",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SSR_ROLES",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "User" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SSR_REVIEWS_UserId",
                table: "SSR_REVIEWS",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SSR_USERS_RoleId",
                table: "SSR_USERS",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SSR_REVIEWS");

            migrationBuilder.DropTable(
                name: "SSR_USERS");

            migrationBuilder.DropTable(
                name: "SSR_ROLES");
        }
    }
}
