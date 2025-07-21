using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueDivision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Divisions_Name",
                table: "Divisions");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Name_CompanyId",
                table: "Divisions",
                columns: new[] { "Name", "CompanyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Divisions_Name_CompanyId",
                table: "Divisions");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Name",
                table: "Divisions",
                column: "Name",
                unique: true);
        }
    }
}
