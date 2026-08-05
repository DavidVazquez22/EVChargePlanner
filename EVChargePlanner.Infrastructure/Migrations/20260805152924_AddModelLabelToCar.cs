using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVChargePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModelLabelToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelLable",
                table: "Cars",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelLable",
                table: "Cars");
        }
    }
}
