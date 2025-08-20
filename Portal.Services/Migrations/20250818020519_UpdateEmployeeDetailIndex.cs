using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Services.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeDetailIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Username",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDetails_EmployeeCode",
                table: "EmployeeDetails");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Username_CompanyId",
                table: "Employees",
                columns: new[] { "Username", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_EmployeeCode",
                table: "EmployeeDetails",
                column: "EmployeeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Username_CompanyId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeDetails_EmployeeCode",
                table: "EmployeeDetails");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Username",
                table: "Employees",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_EmployeeCode",
                table: "EmployeeDetails",
                column: "EmployeeCode",
                unique: true);
        }
    }
}
