using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Portal.Services.Interfaces;
using Portal.Shared.Constants;
using Portal.Shared.Enums;
using Portal.Shared.Models.Entities;
using Portal.Shared.Models.Entities.IT_Inventory;
using Portal.Shared.Models.Entities.Support;

namespace Portal.Services.Models
{
    // ปรับปรุง Constructor หลักให้รับ IHttpContextAccessor
    public class PortalDbContext(DbContextOptions<PortalDbContext> options, ICurrentUserService currentUserService, IHttpContextAccessor httpContextAccessor) : DbContext(options)
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyBranch> CompanyBranches { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeCompanyAccess> EmployeeCompanyAccesses { get; set; }
        public DbSet<EmployeeDetail> EmployeeDetails { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportTicketCategory> SupportTicketCategories { get; set; }
        public DbSet<SupportTicketHistory> SupportTicketHistories { get; set; }

        public DbSet<IT_Item> IT_Items { get; set; }
        public DbSet<IT_Asset> IT_Assets { get; set; }
        public DbSet<IT_Stock> IT_Stocks { get; set; }
        public DbSet<IT_StandardSet> IT_StandardSets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ShortName).HasMaxLength(10).IsRequired();

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.ShortName).IsUnique();
            });

            modelBuilder.Entity<CompanyBranch>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.BranchCode).HasMaxLength(10).IsRequired();

                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => new { e.CompanyId, e.BranchCode }).IsUnique();
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();

                entity.HasIndex(e => e.DivisionId);
                entity.HasIndex(e => new { e.DivisionId, e.Name }).IsUnique();
            });

            modelBuilder.Entity<Division>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();

                entity.HasIndex(e => new { e.Name, e.CompanyId }).IsUnique();
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Username).HasMaxLength(255);
                entity.Property(e => e.EmployeeStatus).HasConversion<int>();

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.DivisionId);
                entity.HasIndex(e => e.DepartmentId);
                entity.HasIndex(e => e.SectionId);
                entity.HasIndex(e => e.EmployeeStatus);
                entity.HasIndex(e => e.CreatedAt);
            });

            modelBuilder.Entity<EmployeeCompanyAccess>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.AccessLevel).HasConversion<int>();

                entity.HasKey(eca => new { eca.EmployeeId, eca.CompanyId, eca.CompanyBranchId });

                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.CompanyBranchId);
                entity.HasIndex(e => e.AccessLevel);
                entity.HasIndex(e => e.GrantedDate);
                entity.HasIndex(e => e.ExpiryDate);

                entity.HasOne(eca => eca.Employee)
                    .WithMany(e => e.EmployeeCompanyAccesses)
                    .HasForeignKey(eca => eca.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(eca => eca.Company)
                    .WithMany(c => c.EmployeeAccesses)
                    .HasForeignKey(eca => eca.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(eca => eca.CompanyBranch)
                    .WithMany(cb => cb.EmployeeAccesses)
                    .HasForeignKey(eca => eca.CompanyBranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EmployeeDetail>(entity =>
            {
                entity.HasOne(eca => eca.Employee)
                    .WithOne(e => e.EmployeeDetail)
                    .HasForeignKey<EmployeeDetail>(ed => ed.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasKey(ed => ed.EmployeeId);
                entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();

                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.FirstName);
                entity.HasIndex(e => e.LastName);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<Section>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.DepartmentId);
                entity.HasIndex(e => new { e.DepartmentId, e.Name }).IsUnique();
            });

            modelBuilder.Entity<UploadedFile>(entity =>
            {
                entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.OriginalFileName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.ContentType).HasMaxLength(255).IsRequired();
                entity.Property(e => e.UploadPath).HasMaxLength(255).IsRequired();
                entity.HasIndex(e => e.UploadDateTime);
                entity.HasIndex(e => e.UploadedByUserId);
                entity.HasIndex(e => e.FileName);
            });

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.ProfilePicture)
                .WithMany()
                .HasForeignKey(e => e.ProfilePictureId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UploadedFile>()
                .HasOne(uf => uf.UploadedByUser)
                .WithMany(e => e.UploadedFiles)
                .HasForeignKey(uf => uf.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- SupportTicket Configuration ---
            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.HasIndex(e => e.TicketNumber).IsUnique();

                entity.Property(e => e.RequestType).HasConversion<string>();
                entity.Property(e => e.Priority).HasConversion<string>();
                entity.Property(e => e.Status).HasConversion<string>();

                // Relationship with Employee (ReportedBy)
                entity.HasOne(st => st.ReportedByEmployee)
                    .WithMany() // Employee can have many tickets
                    .HasForeignKey(st => st.ReportedByEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting employee if they have tickets

                // Relationship with Employee (AssignedTo)
                entity.HasOne(st => st.AssignedToEmployee)
                    .WithMany()
                    .HasForeignKey(st => st.AssignedToEmployeeId)
                    .IsRequired(false) // Can be nullable
                    .OnDelete(DeleteBehavior.SetNull); // If IT staff is deleted, set AssignedTo to null

                // Relationship with IT_Asset
                entity.HasOne(st => st.RelatedAsset)
                      .WithMany()
                      .HasForeignKey(st => st.AssetId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // --- SupportTicketHistory Configuration ---
            modelBuilder.Entity<SupportTicketHistory>(entity =>
            {
                // Relationship with Employee
                entity.HasOne(sth => sth.Employee)
                    .WithMany()
                    .HasForeignKey(sth => sth.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relationship with UploadedFile
                entity.HasOne(sth => sth.FileAttachment)
                      .WithMany()
                      .HasForeignKey(sth => sth.FileAttachmentId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // --- SupportTicketCategory Configuration ---
            modelBuilder.Entity<SupportTicketCategory>(entity =>
            {
                entity.Property(e => e.CategoryType).HasConversion<string>();
            });

            // --- IT_Asset Configuration ---
            modelBuilder.Entity<IT_Asset>(entity =>
            {
                entity.HasIndex(e => e.AssetTag).IsUnique();
                entity.Property(e => e.Status).HasConversion<string>();

                entity.HasOne(a => a.AssignedToEmployee)
                      .WithMany()
                      .HasForeignKey(a => a.AssignedToEmployeeId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // --- IT_Item Configuration ---
            modelBuilder.Entity<IT_Item>(entity =>
            {
                entity.Property(e => e.ItemType).HasConversion<string>();
            });

            // --- IT_StandardSet Configuration ---
            modelBuilder.Entity<IT_StandardSet>(entity =>
            {
                // Ensures one role can only be assigned to one standard set
                entity.HasIndex(e => e.AssignedToRoleId).IsUnique();
            });

            modelBuilder.Entity<Role>().HasData(
                 new Role() { Id = (int)RoleType.Chairman, Name = RoleType.Chairman.GetDisplayName() },
                 new Role() { Id = (int)RoleType.ManagingDirector, Name = RoleType.ManagingDirector.GetDisplayName() },
                 new Role() { Id = (int)RoleType.Secretary, Name = RoleType.Secretary.GetDisplayName() },
                 new Role() { Id = (int)RoleType.DeputyManagingDirector, Name = RoleType.DeputyManagingDirector.GetDisplayName() },
                 new Role() { Id = (int)RoleType.DepartmentManager, Name = RoleType.DepartmentManager.GetDisplayName() },
                 new Role() { Id = (int)RoleType.SectionManager, Name = RoleType.SectionManager.GetDisplayName() },
                 new Role() { Id = (int)RoleType.Staff, Name = RoleType.Staff.GetDisplayName() }
             );

            modelBuilder.Entity<Company>().HasData(
               new Company() { Id = 1, Name = "บริษัทอัคคีปราการ จำกัด (มหาชน)", ShortName = "AKP" }
               );

            modelBuilder.Entity<CompanyBranch>().HasData(
                new CompanyBranch() { Id = 1, Name = "สำนักงานใหญ่", BranchCode = "00", CompanyId = 1 }
                );

            modelBuilder.Entity<Division>().HasData(
                new Division() { Id = 1, CompanyId = 1, Name = "สายงานบริหาร" },
                new Division() { Id = 2, CompanyId = 1, Name = "สายงานบัญชีและการเงิน" },
                new Division() { Id = 3, CompanyId = 1, Name = "สายงานวิชาการ" },
                new Division() { Id = 4, CompanyId = 1, Name = "สายงานปฏิบัติการ" }
                );

            modelBuilder.Entity<Department>().HasData(
                new Department() { Id = 1, DivisionId = 1, Name = "ฝ่ายขายและการตลาด" },
                new Department() { Id = 2, DivisionId = 1, Name = "ฝ่ายขนส่ง" },
                new Department() { Id = 3, DivisionId = 2, Name = "ฝ่ายบริหารองค์กร" },
                new Department() { Id = 4, DivisionId = 2, Name = "ฝ่ายบัญชีและการเงิน" },
                new Department() { Id = 5, DivisionId = 3, Name = "ฝ่ายห้องปฏิบัติการวิเคราะห์" },
                new Department() { Id = 6, DivisionId = 3, Name = "ฝ่ายสิ่งแวดล้อมชีวอนามัย" },
                new Department() { Id = 7, DivisionId = 4, Name = "ฝ่ายซ่อมบำรุง" },
                new Department() { Id = 8, DivisionId = 4, Name = "ฝ่ายตรวจรับและเตรียมของเสีย" },
                new Department() { Id = 9, DivisionId = 4, Name = "ปฏิบัติการ" }
                );

            modelBuilder.Entity<Section>().HasData(
                new Section() { Id = 1, DepartmentId = 1, Name = "การตลาดและสื่อสารองค์กร" },
                new Section() { Id = 2, DepartmentId = 1, Name = "สารสนเทศ" },
                new Section() { Id = 3, DepartmentId = 1, Name = "บริการลูกค้า" },
                new Section() { Id = 4, DepartmentId = 1, Name = "บริหารการขาย" },
                new Section() { Id = 5, DepartmentId = 2, Name = "ขนส่ง" },
                new Section() { Id = 6, DepartmentId = 2, Name = "ซ่อมบำรุงรถขนส่งและภาชนะ" },
                new Section() { Id = 7, DepartmentId = 2, Name = "กำจัดขยะอุตสาหกรรม" },
                new Section() { Id = 8, DepartmentId = 3, Name = "ทรัพยากรบุคคล" },
                new Section() { Id = 9, DepartmentId = 3, Name = "จัดซื้อ" },
                new Section() { Id = 10, DepartmentId = 3, Name = "คลังพัสดุ" },
                new Section() { Id = 11, DepartmentId = 4, Name = "บัญชีและการเงิน" },
                new Section() { Id = 12, DepartmentId = 4, Name = "ติดตามและเร่งรัดหนี้สิน" },
                new Section() { Id = 13, DepartmentId = 5, Name = "ห้องปฏิบัติการวิเคราะห์" },
                new Section() { Id = 14, DepartmentId = 6, Name = "สิ่งแวดล้อม" },
                new Section() { Id = 15, DepartmentId = 6, Name = "อาชีวอนามัยและความปลอดภัย" },
                new Section() { Id = 16, DepartmentId = 7, Name = "ไฟฟ้า" },
                new Section() { Id = 17, DepartmentId = 7, Name = "เครื่องกลและเครื่องยนต์" },
                new Section() { Id = 18, DepartmentId = 7, Name = "วิศวกรรม" },
                new Section() { Id = 19, DepartmentId = 8, Name = "ตรวจรับและจัดเก็บของเสีย" },
                new Section() { Id = 20, DepartmentId = 8, Name = "เตรียมของเสีย" },
                new Section() { Id = 21, DepartmentId = 9, Name = "เผากาก" }
                );

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // OnBeforeSaveChanges is now responsible for creating AuditEntry objects.
            var auditEntries = OnBeforeSaveChanges();

            // Save the actual data changes to the database. This is where temporary keys get their real values.
            var result = await base.SaveChangesAsync(cancellationToken);

            // OnAfterSaveChanges is now responsible for handling the generated keys and saving the logs.
            await OnAfterSaveChanges(auditEntries);

            return result;
        }

        /// <summary>
        /// This method is called by SaveChangesAsync before the changes are committed to the database.
        /// Its main purpose is to detect all changes and create a list of AuditEntry objects.
        /// </summary>
        /// <returns>A list of AuditEntry objects representing the changes.</returns>
        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var entries = new List<AuditEntry>();

            // Create a single, unique TransactionId for this entire batch of changes.
            // This allows grouping all modifications that occur in a single SaveChanges call.
            var transactionId = Guid.NewGuid();

            foreach (var entry in ChangeTracker.Entries())
            {
                // Skip entities that are not changed or not tracked.
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // Do not audit the AuditLog table itself to prevent an infinite loop.
                if (entry.Entity is AuditLog)
                    continue;

                // Create a new AuditEntry for each changed entity.
                var auditEntry = new AuditEntry(entry, entry.Metadata.GetTableName() ?? "Unknown")
                {
                    // Assign the same TransactionId to all entries in this batch.
                    TransactionId = transactionId,
                    UserId = _currentUserService.UserId?.ToString(),
                    Username = _currentUserService.Username,
                    IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString(),
                    TraceId = _httpContextAccessor.HttpContext?.TraceIdentifier
                };

                foreach (var property in entry.Properties)
                {
                    // If the property has a temporary value (e.g., a new auto-generated ID),
                    // store it separately to be processed after the main save operation.
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;

                    // Store the primary key value(s).
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue!;
                        continue;
                    }

                    switch (entry.State)
                    {
                        // For newly added entities, record the new values.
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Added;
                            auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            break;

                        // For deleted entities, record the old values.
                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Deleted;
                            auditEntry.OldValues[propertyName] = property.OriginalValue!;
                            break;

                        // For modified entities, record old and new values for changed properties.
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Modified;
                                auditEntry.OldValues[propertyName] = property.OriginalValue!;
                                auditEntry.NewValues[propertyName] = property.CurrentValue!;
                            }
                            break;
                    }
                }
                entries.Add(auditEntry);
            }
            return entries;
        }

        /// <summary>
        /// This method is called by SaveChangesAsync after the main data changes have been committed.
        /// It updates temporary properties with their final database-generated values and saves the audit logs.
        /// </summary>
        /// <param name="auditEntries">The list of AuditEntry objects created by OnBeforeSaveChanges.</param>
        private async Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            foreach (var auditEntry in auditEntries)
            {
                // Now that the main save is complete, retrieve the final values of temporary properties.
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue!;
                    }
                }

                // Convert the completed AuditEntry to an AuditLog entity and add it to the context.
                AuditLogs.Add(auditEntry.ToAudit());
            }

            // Save the audit logs to the database in a separate transaction.
            // This ensures that even if logging fails, the main data operation is not rolled back.
            await SaveChangesAsync(CancellationToken.None);
        }

    }
}
