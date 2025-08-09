using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Services.Migrations
{
    /// <inheritdoc />
    public partial class SupportTicketFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_SupportTickets_SupportTicketId",
                table: "UploadedFiles");

            migrationBuilder.DropIndex(
                name: "IX_UploadedFiles_SupportTicketId",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "SupportTicketId",
                table: "UploadedFiles");

            migrationBuilder.CreateTable(
                name: "SupportTicketFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupportTicketId = table.Column<int>(type: "int", nullable: false),
                    UploadedFileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketFiles_SupportTickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketFiles_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketFiles_SupportTicketId",
                table: "SupportTicketFiles",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketFiles_UploadedFileId",
                table: "SupportTicketFiles",
                column: "UploadedFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupportTicketFiles");

            migrationBuilder.AddColumn<int>(
                name: "SupportTicketId",
                table: "UploadedFiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_SupportTicketId",
                table: "UploadedFiles",
                column: "SupportTicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_UploadedFiles_SupportTickets_SupportTicketId",
                table: "UploadedFiles",
                column: "SupportTicketId",
                principalTable: "SupportTickets",
                principalColumn: "Id");
        }
    }
}
