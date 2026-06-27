using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AuditArchitectureFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_executions_disciplines_DisciplineId",
                table: "executions");

            migrationBuilder.DropForeignKey(
                name: "FK_executions_teachers_TeacherId",
                table: "executions");

            migrationBuilder.DropIndex(
                name: "IX_executions_DisciplineId",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "DisciplineId",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "Laboratories",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "Lectures",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "OthersWork",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "Practical",
                table: "executions");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "executions",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_executions_TeacherId",
                table: "executions",
                newName: "IX_executions_ScheduleId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "subjects",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "WeekId",
                table: "schedules",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                INSERT INTO weeks ("Name", "StartDate", "EndDate")
                SELECT 'Default migration week', CURRENT_DATE, CURRENT_DATE + INTERVAL '7 days'
                WHERE EXISTS (SELECT 1 FROM schedules)
                  AND NOT EXISTS (SELECT 1 FROM weeks);
                """);

            migrationBuilder.Sql("""
                UPDATE schedules
                SET "WeekId" = (SELECT "Id" FROM weeks ORDER BY "Id" LIMIT 1)
                WHERE "WeekId" IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "WeekId",
                table: "schedules",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExecutionDate",
                table: "executions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "executions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "executions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ResetCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ResetCodeExpires = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RefreshTokenExpires = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalProviderId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_schedules_WeekId",
                table: "schedules",
                column: "WeekId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_executions_schedules_ScheduleId",
                table: "executions",
                column: "ScheduleId",
                principalTable: "schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_schedules_weeks_WeekId",
                table: "schedules",
                column: "WeekId",
                principalTable: "weeks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_executions_schedules_ScheduleId",
                table: "executions");

            migrationBuilder.DropForeignKey(
                name: "FK_schedules_weeks_WeekId",
                table: "schedules");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_schedules_WeekId",
                table: "schedules");

            migrationBuilder.DropColumn(
                name: "WeekId",
                table: "schedules");

            migrationBuilder.DropColumn(
                name: "ExecutionDate",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "executions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "executions");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "executions",
                newName: "TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_executions_ScheduleId",
                table: "executions",
                newName: "IX_executions_TeacherId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "subjects",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<int>(
                name: "DisciplineId",
                table: "executions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Laboratories",
                table: "executions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Lectures",
                table: "executions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OthersWork",
                table: "executions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Practical",
                table: "executions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_executions_DisciplineId",
                table: "executions",
                column: "DisciplineId");

            migrationBuilder.AddForeignKey(
                name: "FK_executions_disciplines_DisciplineId",
                table: "executions",
                column: "DisciplineId",
                principalTable: "disciplines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_executions_teachers_TeacherId",
                table: "executions",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
