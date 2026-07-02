using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearWeekGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_weeks_AcademicYearId",
                table: "weeks");

            migrationBuilder.CreateIndex(
                name: "IX_weeks_AcademicYearId_Name",
                table: "weeks",
                columns: new[] { "AcademicYearId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weeks_AcademicYearId_StartDate_EndDate",
                table: "weeks",
                columns: new[] { "AcademicYearId", "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_weeks_AcademicYearId_Name",
                table: "weeks");

            migrationBuilder.DropIndex(
                name: "IX_weeks_AcademicYearId_StartDate_EndDate",
                table: "weeks");

            migrationBuilder.CreateIndex(
                name: "IX_weeks_AcademicYearId",
                table: "weeks",
                column: "AcademicYearId");
        }
    }
}
