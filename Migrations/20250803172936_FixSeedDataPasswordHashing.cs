using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web_SIMS.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataPasswordHashing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "Password", "PasswordSalt" },
                values: new object[] { "jzHNhDp4sAVMMpeIF9n9xdo7uCs3zAWauxeEpq0u/+o=", "YWRtaW5zYWx0MTIz" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "Password", "PasswordSalt" },
                values: new object[] { "zgSJdBnE9kBtFNh1uQVji6hJoRNXmiHXE2q3IVeQjQs=", "ZmFjdWx0eXNhbHQxMjM=" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "Password", "PasswordSalt" },
                values: new object[] { "knbvQbVpZn68EueH2Wbj5BNGDBNB48ucTZUGacagHUs=", "c3R1ZGVudHNhbHQxMjM=" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                columns: new[] { "Password", "PasswordSalt" },
                values: new object[] { "admin123", "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                columns: new[] { "Password", "PasswordSalt" },
                values: new object[] { "faculty123", "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                columns: new[] { "Password", "PasswordSalt" },
                values: new object[] { "student123", "" });
        }
    }
}
