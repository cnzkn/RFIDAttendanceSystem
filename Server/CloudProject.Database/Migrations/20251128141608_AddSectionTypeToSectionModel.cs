using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudProject.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionTypeToSectionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionType",
                table: "Sections",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectionType",
                table: "Sections");
        }
    }
}
