using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EVChargePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCarModelCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    BatteryCapacityKWh = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxChargingPowerKW = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModels", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CarModels",
                columns: new[] { "Id", "BatteryCapacityKWh", "Brand", "MaxChargingPowerKW", "Model" },
                values: new object[,]
                {
                    { 1, 57.5m, "Tesla", 11m, "Model 3" },
                    { 2, 75m, "Tesla", 11m, "Model Y" },
                    { 3, 58m, "Volkswagen", 11m, "ID.3" },
                    { 4, 77m, "Volkswagen", 11m, "ID.4" },
                    { 5, 60.4m, "BYD", 11m, "Dolphin" },
                    { 6, 60.5m, "BYD", 11m, "Atto 3" },
                    { 7, 65.4m, "Hyundai", 11m, "Kona Electric" },
                    { 8, 77.4m, "Hyundai", 11m, "Ioniq 5" },
                    { 9, 18.1m, "Toyota", 6.6m, "RAV4 Plug-in Hybrid" },
                    { 10, 18.8m, "Volvo", 6.4m, "XC60 Recharge" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarModels");
        }
    }
}
