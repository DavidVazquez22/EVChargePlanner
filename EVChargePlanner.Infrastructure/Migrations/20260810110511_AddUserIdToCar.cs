using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVChargePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Cars",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Cars");
        }
    }
}
