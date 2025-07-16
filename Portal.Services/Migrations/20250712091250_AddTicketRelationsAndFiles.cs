using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketRelationsAndFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupportTicketId",
                table: "UploadedFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedTicketId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_SupportTicketId",
                table: "UploadedFiles",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_RelatedTicketId",
                table: "SupportTickets",
                column: "RelatedTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_SupportTickets_RelatedTicketId",
                table: "SupportTickets",
                column: "RelatedTicketId",
                principalTable: "SupportTickets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFiles_SupportTickets_SupportTicketId",
                table: "UploadedFiles",
                column: "SupportTicketId",
                principalTable: "SupportTickets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_SupportTickets_RelatedTicketId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_SupportTickets_SupportTicketId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_SupportTicketId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_RelatedTicketId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "SupportTicketId",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "RelatedTicketId",
                table: "SupportTickets");
        }
    }
}
