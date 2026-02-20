using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyMapData.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCoordinatesFromNeighborhood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Neighborhoods");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Neighborhoods");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Neighborhoods",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Neighborhoods",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
