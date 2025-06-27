using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Portal.Services.Interfaces;
using Portal.Shared.Constants;
using Portal.Shared.Enums;
using Portal.Shared.Models.Entities;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

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

                entity.HasIndex(e => e.Name).IsUnique();
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

            modelBuilder.Entity<Role>().HasData(
                 new Role() { Id = (int)RoleType.Chairman, Name = RoleType.Chairman.GetDisplayName() },
                 new Role() { Id = (int)RoleType.ManagingDirector, Name = RoleType.ManagingDirector.GetDisplayName() },
                 new Role() { Id = (int)RoleType.Secretary, Name = RoleType.Secretary.GetDisplayName() },
                 new Role() { Id = (int)RoleType.DeputyManagingDirector, Name = RoleType.DeputyManagingDirector.GetDisplayName() },
                 new Role() { Id = (int)RoleType.DepartmentManager, Name = RoleType.DepartmentManager.GetDisplayName() },
                 new Role() { Id = (int)RoleType.Staff, Name = RoleType.Staff.GetDisplayName() }
             );

            modelBuilder.Entity<Company>().HasData(
               new Company() { Id = 1, Name = "บริษัทอัคคีปราการ จำกัด (มหาชน)", ShortName = "AKP" }
               );

            modelBuilder.Entity<CompanyBranch>().HasData(
                new CompanyBranch() { Id = 1, Name = "สำนักงานใหญ่", BranchCode = "00", CompanyId = 1 }
                );

            modelBuilder.Entity<Division>().HasData(
                new Division() { Id = 1, Name = "สายงานบริหาร" },
                new Division() { Id = 2, Name = "สายงานบัญชีและการเงิน" },
                new Division() { Id = 3, Name = "สายงานวิชาการ" },
                new Division() { Id = 4, Name = "สายงานปฏิบัติการ" }
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
            var auditEntries = HandleAuditEntries();

            HandleAuditableAndSoftDeletableEntities();

            if (auditEntries.Count != 0)
            {
                await AuditLogs.AddRangeAsync(auditEntries.Select(e => e.ToAudit()), cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private List<AuditEntry> HandleAuditEntries()
        {
            ChangeTracker.DetectChanges();
            var entries = new List<AuditEntry>();
            var httpContext = _httpContextAccessor.HttpContext;

            foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is not AuditLog && e.State != EntityState.Detached && e.State != EntityState.Unchanged))
            {
                // 1. ดึง tableName ออกมาก่อน
                var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;

                // 2. ส่ง tableName เข้าไปใน Constructor ตามที่คุณได้ออกแบบไว้
                var auditEntry = new AuditEntry(entry, tableName)
                {
                    // 3. ไม่ต้องกำหนดค่า TableName ที่นี่อีกต่อไป
                    UserId = _currentUserService.UserId?.ToString(),
                    Username = _currentUserService.Username,
                    IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                    UserAgent = httpContext?.Request?.Headers.UserAgent,
                    TraceId = httpContext?.TraceIdentifier
                };

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue;

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue ?? DBNull.Value;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Added;
                            auditEntry.NewValues[propertyName] = property.CurrentValue ?? DBNull.Value;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Deleted;
                            auditEntry.OldValues[propertyName] = property.OriginalValue ?? System.DBNull.Value;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified && ((property.OriginalValue != null && !property.OriginalValue.Equals(property.CurrentValue)) || (property.OriginalValue == null && property.CurrentValue != null)))
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Modified;
                                auditEntry.OldValues[propertyName] = property.OriginalValue ?? System.DBNull.Value;
                                auditEntry.NewValues[propertyName] = property.CurrentValue ?? DBNull.Value;
                            }
                            break;
                    }
                }

                if (auditEntry.AuditType != AuditType.None)
                {
                    entries.Add(auditEntry);
                }
            }
            return entries;
        }


        private void HandleAuditableAndSoftDeletableEntities()
        {
            var now = DateTime.UtcNow;
            var currentUserId = _currentUserService.UserId;
            var currentUserName = _currentUserService.Username;

            foreach (var entry in ChangeTracker.Entries<IAuditable>().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.CreatedByName = currentUserId.HasValue ? (currentUserName ?? "Unknown User") : "System/Process";
                }
                else
                {
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    entry.Entity.UpdatedByName = currentUserId.HasValue ? (currentUserName ?? "Unknown User") : "System/Process";
                }
            }

            foreach (var entry in ChangeTracker.Entries<ISoftDeletable>().Where(e => e.State == EntityState.Deleted))
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = now;
                entry.Entity.DeletedBy = currentUserId;
            }
        }
    }
}
