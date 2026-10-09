using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenToWork.Models.Migrations
{
    /// <inheritdoc />
    public partial class VacancyGeoCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GeocodedAt",
                table: "PT_Vacancies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "PT_Vacancies",
                type: "double",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "PT_Vacancies",
                type: "double",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_Vacancies_Latitude_Longitude_Status_IsDeleted",
                table: "PT_Vacancies",
                columns: new[] { "Latitude", "Longitude", "Status", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PT_Vacancies_Latitude_Longitude_Status_IsDeleted",
                table: "PT_Vacancies");

            migrationBuilder.DropColumn(
                name: "GeocodedAt",
                table: "PT_Vacancies");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "PT_Vacancies");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "PT_Vacancies");
        }
    }
}
