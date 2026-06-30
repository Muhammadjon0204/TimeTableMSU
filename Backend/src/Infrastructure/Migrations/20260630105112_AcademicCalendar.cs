using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AcademicCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicPeriodId",
                table: "weeks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcademicYearId",
                table: "weeks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "weeks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "weeks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Study");

            migrationBuilder.CreateTable(
                name: "academic_years",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_years", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "academic_periods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_academic_periods_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcademicYearId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    IsRecurringByDate = table.Column<bool>(type: "boolean", nullable: false),
                    IsStudyBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_holidays_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weeks_AcademicPeriodId",
                table: "weeks",
                column: "AcademicPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_weeks_AcademicYearId",
                table: "weeks",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_academic_periods_AcademicYearId_StartDate_EndDate",
                table: "academic_periods",
                columns: new[] { "AcademicYearId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_academic_years_IsCurrent",
                table: "academic_years",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_academic_years_Name",
                table: "academic_years",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_holidays_AcademicYearId_Date",
                table: "holidays",
                columns: new[] { "AcademicYearId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_weeks_academic_periods_AcademicPeriodId",
                table: "weeks",
                column: "AcademicPeriodId",
                principalTable: "academic_periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_weeks_academic_years_AcademicYearId",
                table: "weeks",
                column: "AcademicYearId",
                principalTable: "academic_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_weeks_academic_periods_AcademicPeriodId",
                table: "weeks");

            migrationBuilder.DropForeignKey(
                name: "FK_weeks_academic_years_AcademicYearId",
                table: "weeks");

            migrationBuilder.DropTable(
                name: "academic_periods");

            migrationBuilder.DropTable(
                name: "holidays");

            migrationBuilder.DropTable(
                name: "academic_years");

            migrationBuilder.DropIndex(
                name: "IX_weeks_AcademicPeriodId",
                table: "weeks");

            migrationBuilder.DropIndex(
                name: "IX_weeks_AcademicYearId",
                table: "weeks");

            migrationBuilder.DropColumn(
                name: "AcademicPeriodId",
                table: "weeks");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "weeks");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "weeks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "weeks");
        }
    }
}
