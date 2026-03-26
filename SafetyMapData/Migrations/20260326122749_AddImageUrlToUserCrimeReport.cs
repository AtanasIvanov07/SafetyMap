using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyMapData.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlToUserCrimeReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "UserCrimeReports",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "UserCrimeReports");
        }
    }
}
