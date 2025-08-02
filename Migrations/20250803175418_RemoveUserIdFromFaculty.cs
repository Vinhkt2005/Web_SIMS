using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_SIMS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdFromFaculty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Faculties");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Faculties",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Faculties",
                keyColumn: "FacultyId",
                keyValue: 1,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Faculties",
                keyColumn: "FacultyId",
                keyValue: 2,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Faculties",
                keyColumn: "FacultyId",
                keyValue: 3,
                column: "UserId",
                value: null);
        }
    }
}
