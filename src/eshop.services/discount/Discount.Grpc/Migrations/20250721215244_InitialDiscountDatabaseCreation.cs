using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Discount.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class InitialDiscountDatabaseCreation : Migration
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
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AllowStacking = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxStackPercentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    AutoApply = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDisabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscountTier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CouponId = table.Column<int>(type: "INTEGER", nullable: false),
                    ThresholdAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    FixedAmount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountTier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountTier_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Coupon",
                columns: new[] { "Id", "AllowStacking", "AutoApply", "Category", "Code", "Description", "EndDate", "FixedAmount", "IsDisabled", "MaxStackPercentage", "MinimumAmount", "Percentage", "ProductId", "ProductName", "StartDate", "Status", "Type" },
                values: new object[,]
                {
                    { 1, false, true, string.Empty, string.Empty, "Back to school 10%", new DateTimeOffset(new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 0m, false, 0.30m, 0m, 0.10m, new Guid("11111111-1111-1111-1111-111111111111"), "IPhone 15 Pro", new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 0, 0 },
                    { 2, true, true, "Electronics", string.Empty, "Electronics seasonal sale", new DateTimeOffset(new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 0m, false, 0.30m, 100m, 0m, null, string.Empty, new DateTimeOffset(new DateTime(2023, 12, 17, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 0, 0 },
                    { 3, true, false, string.Empty, "SUMMER2024", "Summer code 10% + 5€", new DateTimeOffset(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 5m, false, 0.30m, 50m, 0.10m, null, string.Empty, new DateTimeOffset(new DateTime(2023, 12, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 0, 1 },
                    { 4, false, true, "Home", string.Empty, "Future home discount", new DateTimeOffset(new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 0m, false, 0.30m, 0m, 0.15m, null, string.Empty, new DateTimeOffset(new DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0L)), 3, 0 }
                });

            migrationBuilder.InsertData(
                table: "DiscountTier",
                columns: new[] { "Id", "CouponId", "FixedAmount", "Percentage", "ThresholdAmount" },
                values: new object[,]
                {
                    { 1, 2, 0m, 0.05m, 100m },
                    { 2, 2, 0m, 0.10m, 200m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountTier_CouponId",
                table: "DiscountTier",
                column: "CouponId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountTier");

            migrationBuilder.DropTable(
                name: "Coupon");
        }
    }
}
