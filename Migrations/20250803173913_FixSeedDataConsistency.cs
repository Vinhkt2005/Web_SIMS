using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_SIMS.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "Email", "FacultyId", "FullName" },
                values: new object[] { "nguyen.giang@university.edu.vn", 1, "TS. Nguyễn Văn Giảng" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "nguyenvana@sims.edu", "Nguyễn Văn A" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "Email", "FacultyId", "FullName" },
                values: new object[] { "faculty1@sims.edu", null, "Giảng viên 1" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "Email", "FullName" },
                values: new object[] { "student1@sims.edu", "Sinh viên 1" });
        }
    }
}
