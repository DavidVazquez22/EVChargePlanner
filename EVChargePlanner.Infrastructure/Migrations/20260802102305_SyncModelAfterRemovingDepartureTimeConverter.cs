using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVChargePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAfterRemovingDepartureTimeConverter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentBatteryPercentage",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "DepartureTime",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "TargetBatteryPercentage",
                table: "Cars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentBatteryPercentage",
                table: "Cars",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureTime",
                table: "Cars",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetBatteryPercentage",
                table: "Cars",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
