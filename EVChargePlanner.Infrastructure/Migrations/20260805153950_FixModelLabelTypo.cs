using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVChargePlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixModelLabelTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModelLable",
                table: "Cars",
                newName: "ModelLabel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ModelLabel",
                table: "Cars",
                newName: "ModelLable");
        }
    }
}
