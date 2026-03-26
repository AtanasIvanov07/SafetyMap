using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyMapData.Migrations
{
    /// <inheritdoc />
    public partial class MultipleImagesForCrimeReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "UserCrimeReports");

            migrationBuilder.CreateTable(
                name: "UserCrimeReportImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserCrimeReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCrimeReportImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCrimeReportImages_UserCrimeReports_UserCrimeReportId",
                        column: x => x.UserCrimeReportId,
                        principalTable: "UserCrimeReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCrimeReportImages_UserCrimeReportId",
                table: "UserCrimeReportImages",
                column: "UserCrimeReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCrimeReportImages");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "UserCrimeReports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
