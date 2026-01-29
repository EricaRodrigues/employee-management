using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "BirthDate", "DocNumber", "Email", "FirstName", "LastName", "ManagerId", "PasswordHash", "Role" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1992, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc), "00000000001", "admin@company.com", "Admin", "Director", null, "$2a$11$Ey8TKH0BmJnmnsg1ei30OuG0.N9CdgxGWaDiTtCwFzLN9p2fBMIh6", 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
