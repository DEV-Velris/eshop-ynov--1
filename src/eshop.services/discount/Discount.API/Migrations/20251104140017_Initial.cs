using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Discount.API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coupon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxPercentageCap = table.Column<decimal>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    StartsAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupon", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Coupon",
                columns: new[] { "Id", "Amount", "Category", "Code", "Description", "ExpiresAt", "IsActive", "MaxPercentageCap", "ProductName", "StartsAt", "Type" },
                values: new object[,]
                {
                    { 1, 150m, null, "IPHONE-X-NEW", "IPhone X New", null, true, null, "IPhone X", null, 0 },
                    { 2, 1000m, null, "SAMSUNG-10-NEW", "Samsung 10 New", null, true, null, "Samsung 10", null, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coupon");
        }
    }
}
