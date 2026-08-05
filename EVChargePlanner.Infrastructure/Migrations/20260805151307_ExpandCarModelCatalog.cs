using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EVChargePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCarModelCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CarModels",
                columns: new[] { "Id", "BatteryCapacityKWh", "Brand", "MaxChargingPowerKW", "Model" },
                values: new object[,]
                {
                    { 11, 77.4m, "Kia", 11m, "EV6" },
                    { 12, 64.8m, "Kia", 11m, "Niro EV" },
                    { 13, 59m, "Nissan", 6.6m, "Leaf" },
                    { 14, 63m, "Nissan", 7.4m, "Ariya" },
                    { 15, 77m, "Skoda", 11m, "Enyaq" },
                    { 16, 76.6m, "Audi", 11m, "Q4 e-tron" },
                    { 17, 93.4m, "Audi", 11m, "e-tron GT" },
                    { 18, 83.9m, "BMW", 11m, "i4" },
                    { 19, 74m, "BMW", 11m, "iX3" },
                    { 20, 66.5m, "Mercedes-Benz", 11m, "EQA" },
                    { 21, 80m, "Mercedes-Benz", 11m, "EQC" },
                    { 22, 78m, "Polestar", 11m, "2" },
                    { 23, 77m, "Volkswagen", 11m, "ID.5" },
                    { 24, 35.8m, "Volkswagen", 7.2m, "e-Golf" },
                    { 25, 52m, "Renault", 22m, "Zoe" },
                    { 26, 60m, "Renault", 22m, "Megane E-Tech" },
                    { 27, 54m, "Peugeot", 11m, "e-2008" },
                    { 28, 88m, "Ford", 11m, "Mustang Mach-E" },
                    { 29, 71.4m, "Toyota", 11m, "bZ4X" },
                    { 30, 64m, "Volvo", 11m, "EX30" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "CarModels",
                keyColumn: "Id",
                keyValue: 30);
        }
    }
}
