using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportAndInventorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "AuditLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "IT_Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStockItem = table.Column<bool>(type: "bit", nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IT_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IT_Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssignedToEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IT_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IT_Assets_Employees_AssignedToEmployeeId",
                        column: x => x.AssignedToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IT_Assets_IT_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "IT_Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IT_StandardSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SetName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    AssignedToRoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IT_StandardSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IT_StandardSets_IT_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "IT_Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IT_StandardSets_Roles_AssignedToRoleId",
                        column: x => x.AssignedToRoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IT_Stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IT_Stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IT_Stocks_IT_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "IT_Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: true),
                    RequestedItemId = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Employees_AssignedToEmployeeId",
                        column: x => x.AssignedToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Employees_ReportedByEmployeeId",
                        column: x => x.ReportedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_IT_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "IT_Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupportTickets_IT_Items_RequestedItemId",
                        column: x => x.RequestedItemId,
                        principalTable: "IT_Items",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportTicketCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SupportTicketCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionDescription = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileAttachmentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTicketHistories_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTicketHistories_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportTicketHistories_UploadedFiles_FileAttachmentId",
                        column: x => x.FileAttachmentId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IT_Assets_AssetTag",
                table: "IT_Assets",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IT_Assets_AssignedToEmployeeId",
                table: "IT_Assets",
                column: "AssignedToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_IT_Assets_ItemId",
                table: "IT_Assets",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IT_StandardSets_AssignedToRoleId",
                table: "IT_StandardSets",
                column: "AssignedToRoleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IT_StandardSets_ItemId",
                table: "IT_StandardSets",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_IT_Stocks_ItemId",
                table: "IT_Stocks",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketHistories_EmployeeId",
                table: "SupportTicketHistories",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketHistories_FileAttachmentId",
                table: "SupportTicketHistories",
                column: "FileAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketHistories_TicketId",
                table: "SupportTicketHistories",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssetId",
                table: "SupportTickets",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedToEmployeeId",
                table: "SupportTickets",
                column: "AssignedToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CategoryId",
                table: "SupportTickets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_ReportedByEmployeeId",
                table: "SupportTickets",
                column: "ReportedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_RequestedItemId",
                table: "SupportTickets",
                column: "RequestedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TicketNumber",
                table: "SupportTickets",
                column: "TicketNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IT_StandardSets");

            migrationBuilder.DropTable(
                name: "IT_Stocks");

            migrationBuilder.DropTable(
                name: "SupportTicketHistories");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "IT_Assets");

            migrationBuilder.DropTable(
                name: "SupportTicketCategories");

            migrationBuilder.DropTable(
                name: "IT_Items");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "AuditLogs");
        }
    }
}
