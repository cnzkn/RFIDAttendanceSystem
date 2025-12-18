using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudProject.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWeekNumberToAttendanceLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WeekNumber",
                table: "AttendanceLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeekNumber",
                table: "AttendanceLogs");
        }
    }
}
