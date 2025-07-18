using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Portal.Services.Migrations
{
    /// <inheritdoc />
    public partial class InitialTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IT_Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsStockItem = table.Column<bool>(type: "bit", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IT_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                name: "CompanyBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyBranches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Divisions_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
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
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sections_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TraceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCompanyAccesses",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CompanyBranchId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    GrantedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCompanyAccesses", x => new { x.EmployeeId, x.CompanyId, x.CompanyBranchId });
                    table.ForeignKey(
                        name: "FK_EmployeeCompanyAccesses_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCompanyAccesses_CompanyBranches_CompanyBranchId",
                        column: x => x.CompanyBranchId,
                        principalTable: "CompanyBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDetails",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocalFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LocalLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDetails", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsAdUser = table.Column<bool>(type: "bit", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    SectionId = table.Column<int>(type: "int", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeStatus = table.Column<int>(type: "int", nullable: false),
                    ProfilePictureId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employees_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employees_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employees_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id");
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
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedTicketId = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportTickets_RelatedTicketId",
                        column: x => x.RelatedTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadPath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UploadDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportTicketId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_Employees_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_SupportTickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id");
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

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Name", "ShortName" },
                values: new object[] { 1, "บริษัทอัคคีปราการ จำกัด (มหาชน)", "AKP" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, null, "ประธานกรรมการบริหาร" },
                    { 2, null, "กรรมการผู้จัดการ" },
                    { 3, null, "เลขานุการ" },
                    { 4, null, "รองกรรมการผู้จัดการ" },
                    { 5, null, "ผู้จัดการฝ่าย" },
                    { 6, null, "หัวหน้าแผนก" },
                    { 7, null, "เจ้าหน้าที่ทั่วไป" }
                });

            migrationBuilder.InsertData(
                table: "CompanyBranches",
                columns: new[] { "Id", "BranchCode", "CompanyId", "Name" },
                values: new object[] { 1, "00", 1, "สำนักงานใหญ่" });

            migrationBuilder.InsertData(
                table: "Divisions",
                columns: new[] { "Id", "CompanyId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "สายงานบริหาร" },
                    { 2, 1, "สายงานบัญชีและการเงิน" },
                    { 3, 1, "สายงานวิชาการ" },
                    { 4, 1, "สายงานปฏิบัติการ" }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DivisionId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "ฝ่ายขายและการตลาด" },
                    { 2, 1, "ฝ่ายขนส่ง" },
                    { 3, 2, "ฝ่ายบริหารองค์กร" },
                    { 4, 2, "ฝ่ายบัญชีและการเงิน" },
                    { 5, 3, "ฝ่ายห้องปฏิบัติการวิเคราะห์" },
                    { 6, 3, "ฝ่ายสิ่งแวดล้อมชีวอนามัย" },
                    { 7, 4, "ฝ่ายซ่อมบำรุง" },
                    { 8, 4, "ฝ่ายตรวจรับและเตรียมของเสีย" },
                    { 9, 4, "ปฏิบัติการ" }
                });

            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "DepartmentId", "Name" },
                values: new object[,]
                {
                    { 1, 1, "การตลาดและสื่อสารองค์กร" },
                    { 2, 1, "สารสนเทศ" },
                    { 3, 1, "บริการลูกค้า" },
                    { 4, 1, "บริหารการขาย" },
                    { 5, 2, "ขนส่ง" },
                    { 6, 2, "ซ่อมบำรุงรถขนส่งและภาชนะ" },
                    { 7, 2, "กำจัดขยะอุตสาหกรรม" },
                    { 8, 3, "ทรัพยากรบุคคล" },
                    { 9, 3, "จัดซื้อ" },
                    { 10, 3, "คลังพัสดุ" },
                    { 11, 4, "บัญชีและการเงิน" },
                    { 12, 4, "ติดตามและเร่งรัดหนี้สิน" },
                    { 13, 5, "ห้องปฏิบัติการวิเคราะห์" },
                    { 14, 6, "สิ่งแวดล้อม" },
                    { 15, 6, "อาชีวอนามัยและความปลอดภัย" },
                    { 16, 7, "ไฟฟ้า" },
                    { 17, 7, "เครื่องกลและเครื่องยนต์" },
                    { 18, 7, "วิศวกรรม" },
                    { 19, 8, "ตรวจรับและจัดเก็บของเสีย" },
                    { 20, 8, "เตรียมของเสีย" },
                    { 21, 9, "เผากาก" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmployeeId",
                table: "AuditLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name",
                table: "Companies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ShortName",
                table: "Companies",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CompanyId",
                table: "CompanyBranches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyBranches_CompanyId_BranchCode",
                table: "CompanyBranches",
                columns: new[] { "CompanyId", "BranchCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DivisionId",
                table: "Departments",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DivisionId_Name",
                table: "Departments",
                columns: new[] { "DivisionId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_CompanyId",
                table: "Divisions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Name",
                table: "Divisions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyAccesses_AccessLevel",
                table: "EmployeeCompanyAccesses",
                column: "AccessLevel");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyAccesses_CompanyBranchId",
                table: "EmployeeCompanyAccesses",
                column: "CompanyBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyAccesses_CompanyId",
                table: "EmployeeCompanyAccesses",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyAccesses_EmployeeId",
                table: "EmployeeCompanyAccesses",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyAccesses_ExpiryDate",
                table: "EmployeeCompanyAccesses",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCompanyAccesses_GrantedDate",
                table: "EmployeeCompanyAccesses",
                column: "GrantedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_Email",
                table: "EmployeeDetails",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_EmployeeCode",
                table: "EmployeeDetails",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_FirstName",
                table: "EmployeeDetails",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDetails_LastName",
                table: "EmployeeDetails",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyId",
                table: "Employees",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedAt",
                table: "Employees",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DivisionId",
                table: "Employees",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeStatus",
                table: "Employees",
                column: "EmployeeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ProfilePictureId",
                table: "Employees",
                column: "ProfilePictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RoleId",
                table: "Employees",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_SectionId",
                table: "Employees",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Username",
                table: "Employees",
                column: "Username",
                unique: true);

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
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId",
                table: "Sections",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DepartmentId_Name",
                table: "Sections",
                columns: new[] { "DepartmentId", "Name" },
                unique: true);

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
                name: "IX_SupportTickets_RelatedTicketId",
                table: "SupportTickets",
                column: "RelatedTicketId");

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

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_FileName",
                table: "UploadedFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_SupportTicketId",
                table: "UploadedFiles",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UploadDateTime",
                table: "UploadedFiles",
                column: "UploadDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_UploadedByUserId",
                table: "UploadedFiles",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Employees_EmployeeId",
                table: "AuditLogs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeCompanyAccesses_Employees_EmployeeId",
                table: "EmployeeCompanyAccesses",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeDetails_Employees_EmployeeId",
                table: "EmployeeDetails",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_UploadedFiles_ProfilePictureId",
                table: "Employees",
                column: "ProfilePictureId",
                principalTable: "UploadedFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IT_Assets_Employees_AssignedToEmployeeId",
                table: "IT_Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Employees_AssignedToEmployeeId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Employees_ReportedByEmployeeId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_UploadedFiles_Employees_UploadedByUserId",
                table: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "EmployeeCompanyAccesses");

            migrationBuilder.DropTable(
                name: "EmployeeDetails");

            migrationBuilder.DropTable(
                name: "IT_StandardSets");

            migrationBuilder.DropTable(
                name: "IT_Stocks");

            migrationBuilder.DropTable(
                name: "SupportTicketHistories");

            migrationBuilder.DropTable(
                name: "CompanyBranches");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "IT_Assets");

            migrationBuilder.DropTable(
                name: "SupportTicketCategories");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "IT_Items");
        }
    }
}
