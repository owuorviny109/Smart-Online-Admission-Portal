using Microsoft.EntityFrameworkCore;
using SOAP.Web.Models.Entities;
using SOAP.Web.Data.Configurations;

namespace SOAP.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Core entities
        public DbSet<User> Users { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<SchoolStudent> SchoolStudents { get; set; }
        public DbSet<SmsLog> SmsLogs { get; set; }

        // Security and audit entities
        public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }
        public DbSet<DataProcessingConsent> DataProcessingConsents { get; set; }
        public DbSet<SecurityIncidentRecord> SecurityIncidents { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new SchoolConfiguration());
            modelBuilder.ApplyConfiguration(new ApplicationConfiguration());

            // Configure SchoolStudent relationships
            modelBuilder.Entity<SchoolStudent>()
                .HasOne(ss => ss.School)
                .WithMany(s => s.SchoolStudents)
                .HasForeignKey(ss => ss.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SchoolStudent>()
                .HasIndex(ss => new { ss.KcpeIndexNumber, ss.SchoolId, ss.Year })
                .IsUnique()
                .HasDatabaseName("IX_SchoolStudents_KcpeNumber_School_Year");

            // Configure Document relationships with security
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Application)
                .WithMany(a => a.Documents)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasIndex(d => new { d.ApplicationId, d.DocumentType })
                .HasDatabaseName("IX_Documents_Application_Type");

            // Configure SmsLog relationships
            modelBuilder.Entity<SmsLog>()
                .HasOne(s => s.Application)
                .WithMany()
                .HasForeignKey(s => s.ApplicationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SmsLog>()
                .HasIndex(s => new { s.PhoneNumber, s.CreatedAt })
                .HasDatabaseName("IX_SmsLogs_Phone_Date");

            modelBuilder.Entity<SmsLog>()
                .HasIndex(s => s.Status)
                .HasDatabaseName("IX_SmsLogs_Status");

            // Configure Security Audit Log
            modelBuilder.Entity<SecurityAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.UserRole).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 support
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ResourceAccessed).HasMaxLength(200);
                entity.Property(e => e.ActionPerformed).HasMaxLength(100);
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.AdditionalData).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.EventType, e.Timestamp })
                    .HasDatabaseName("IX_SecurityAuditLogs_EventType_Timestamp");
                entity.HasIndex(e => new { e.UserId, e.Timestamp })
                    .HasDatabaseName("IX_SecurityAuditLogs_UserId_Timestamp");
                entity.HasIndex(e => new { e.IpAddress, e.Timestamp })
                    .HasDatabaseName("IX_SecurityAuditLogs_IpAddress_Timestamp");
            });

            // Configure Data Processing Consent
            modelBuilder.Entity<DataProcessingConsent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ConsentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Purpose).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ConsentVersion).IsRequired().HasMaxLength(10);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ConsentDate).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.UserId, e.ConsentType })
                    .HasDatabaseName("IX_DataProcessingConsents_User_Type");
            });

            // Configure Security Incident Record
            modelBuilder.Entity<SecurityIncidentRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.IncidentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Severity).IsRequired();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.AffectedUserId).HasMaxLength(450);
                entity.Property(e => e.SourceIpAddress).HasMaxLength(45);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AutomaticResponse).HasMaxLength(500);
                entity.Property(e => e.DetectedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.IncidentType, e.DetectedAt })
                    .HasDatabaseName("IX_SecurityIncidents_Type_Date");
                entity.HasIndex(e => new { e.Status, e.Severity })
                    .HasDatabaseName("IX_SecurityIncidents_Status_Severity");
            });

            // Configure Login Attempt tracking
            modelBuilder.Entity<LoginAttempt>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Success).IsRequired();
                entity.Property(e => e.FailureReason).HasMaxLength(200);
                entity.Property(e => e.AttemptedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => new { e.PhoneNumber, e.AttemptedAt })
                    .HasDatabaseName("IX_LoginAttempts_Phone_Date");
                entity.HasIndex(e => new { e.IpAddress, e.AttemptedAt })
                    .HasDatabaseName("IX_LoginAttempts_IP_Date");
                entity.HasIndex(e => new { e.Success, e.AttemptedAt })
                    .HasDatabaseName("IX_LoginAttempts_Success_Date");
            });

            // Configure performance indexes for core entities
            ConfigurePerformanceIndexes(modelBuilder);

            // Configure data protection and encryption
            ConfigureDataProtection(modelBuilder);
        }

        private void ConfigurePerformanceIndexes(ModelBuilder modelBuilder)
        {
            // Application performance indexes
            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.SchoolId, a.Status, a.CreatedAt })
                .HasDatabaseName("IX_Applications_School_Status_Date");

            modelBuilder.Entity<Application>()
                .HasIndex(a => new { a.Status, a.UpdatedAt })
                .HasDatabaseName("IX_Applications_Status_Updated");

            // User performance indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.Role, u.IsActive })
                .HasDatabaseName("IX_Users_Role_Active");

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.SchoolId, u.Role })
                .HasDatabaseName("IX_Users_School_Role");

            // School performance indexes
            modelBuilder.Entity<School>()
                .HasIndex(s => new { s.County, s.IsActive })
                .HasDatabaseName("IX_Schools_County_Active");
        }

        private void ConfigureDataProtection(ModelBuilder modelBuilder)
        {
            // Configure sensitive data fields for encryption
            // These will be handled by the DataProtectionService
            
            // Mark sensitive fields in Application entity
            modelBuilder.Entity<Application>()
                .Property(a => a.ParentPhone)
                .HasComment("Encrypted personal data");

            modelBuilder.Entity<Application>()
                .Property(a => a.HomeAddress)
                .HasComment("Encrypted personal data");

            modelBuilder.Entity<Application>()
                .Property(a => a.MedicalConditions)
                .HasComment("Encrypted personal data");

            // Configure row-level security policies (for future implementation)
            modelBuilder.Entity<Application>()
                .HasComment("Row-level security: Users can only access applications from their school");

            modelBuilder.Entity<Document>()
                .HasComment("Row-level security: Users can only access documents from applications they own or manage");
        }

        // Override SaveChanges to add audit logging
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await AddAuditLogsAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            AddAuditLogsAsync().GetAwaiter().GetResult();
            return base.SaveChanges();
        }

        private async Task AddAuditLogsAsync()
        {
            var auditEntries = new List<SecurityAuditLog>();
            
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is SecurityAuditLog || entry.State == EntityState.Unchanged)
                    continue;

                var auditLog = new SecurityAuditLog
                {
                    EventType = $"DATA_{entry.State.ToString().ToUpper()}",
                    ResourceAccessed = entry.Entity.GetType().Name,
                    ActionPerformed = entry.State.ToString(),
                    Success = true,
                    Timestamp = DateTimeOffset.UtcNow,
                    AdditionalData = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        EntityType = entry.Entity.GetType().Name,
                        EntityId = GetEntityId(entry),
                        Changes = GetChanges(entry)
                    })
                };

                auditEntries.Add(auditLog);
            }

            if (auditEntries.Any())
            {
                await SecurityAuditLogs.AddRangeAsync(auditEntries);
            }
        }

        private object GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return keyProperty?.CurrentValue;
        }

        private Dictionary<string, object> GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var changes = new Dictionary<string, object>();
            
            foreach (var property in entry.Properties)
            {
                if (property.IsModified)
                {
                    changes[property.Metadata.Name] = new
                    {
                        OldValue = property.OriginalValue,
                        NewValue = property.CurrentValue
                    };
                }
            }
            
            return changes;
        }
    }
}