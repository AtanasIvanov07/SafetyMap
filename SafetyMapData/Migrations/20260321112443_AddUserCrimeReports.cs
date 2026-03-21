using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafetyMapData.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCrimeReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCrimeReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    DateOfIncident = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CrimeCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NeighborhoodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCrimeReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCrimeReports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCrimeReports_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCrimeReports_CrimeCategories_CrimeCategoryId",
                        column: x => x.CrimeCategoryId,
                        principalTable: "CrimeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCrimeReports_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCrimeReports_CityId",
                table: "UserCrimeReports",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCrimeReports_CrimeCategoryId",
                table: "UserCrimeReports",
                column: "CrimeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCrimeReports_NeighborhoodId",
                table: "UserCrimeReports",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCrimeReports_UserId",
                table: "UserCrimeReports",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCrimeReports");
        }
    }
}
