using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;

namespace SOAP.Web.Services.BackgroundServices
{
    public class DataRetentionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataRetentionService> _logger;

        public DataRetentionService(IServiceProvider serviceProvider, ILogger<DataRetentionService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Retention Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Run data retention tasks daily at 2 AM
                    var now = DateTime.Now;
                    var nextRun = DateTime.Today.AddDays(1).AddHours(2);
                    var delay = nextRun - now;

                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }

                    await PurgeOldAuditLogsAsync(context);
                    await ArchiveOldApplicationsAsync(context);
                    await CleanupOldLoginAttemptsAsync(context);
                    await CleanupResolvedSecurityIncidentsAsync(context);
                    await CleanupTemporaryFilesAsync();

                    _logger.LogInformation("Data retention tasks completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Data Retention Service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Data Retention Service stopped");
        }

        private async Task PurgeOldAuditLogsAsync(ApplicationDbContext context)
        {
            // Keep audit logs for 7 years as per legal requirements
            var auditRetentionDate = DateTimeOffset.UtcNow.AddYears(-7);
            
            var oldAuditLogs = await context.SecurityAuditLogs
                .Where(log => log.Timestamp < auditRetentionDate)
                .ToListAsync();

            if (oldAuditLogs.Any())
            {
                context.SecurityAuditLogs.RemoveRange(oldAuditLogs);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Purged {Count} old audit log entries older than {Date}", 
                    oldAuditLogs.Count, auditRetentionDate);
            }
        }

        private async Task ArchiveOldApplicationsAsync(ApplicationDbContext context)
        {
            // Archive completed applications older than 5 years
            var archiveDate = DateTimeOffset.UtcNow.AddYears(-5);
            
            var oldApplications = await context.Applications
                .Where(a => a.Status == "Completed" && a.CreatedAt < archiveDate)
                .Include(a => a.Documents)
                .ToListAsync();

            foreach (var application in oldApplications)
            {
                // In a real implementation, you would move these to an archive database
                // For now, we'll just mark them as archived
                application.Status = "Archived";
                application.UpdatedAt = DateTimeOffset.UtcNow;

                // Optionally, move document files to archive storage
                foreach (var document in application.Documents)
                {
                    // Archive document files
                    await ArchiveDocumentFileAsync(document.FilePath);
                }
            }

            if (oldApplications.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Archived {Count} old applications older than {Date}", 
                    oldApplications.Count, archiveDate);
            }
        }

        private async Task CleanupOldLoginAttemptsAsync(ApplicationDbContext context)
        {
            // Keep login attempts for 90 days
            var cleanupDate = DateTimeOffset.UtcNow.AddDays(-90);
            
            var oldLoginAttempts = await context.LoginAttempts
                .Where(la => la.AttemptedAt < cleanupDate)
                .ToListAsync();

            if (oldLoginAttempts.Any())
            {
                context.LoginAttempts.RemoveRange(oldLoginAttempts);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Cleaned up {Count} old login attempts older than {Date}", 
                    oldLoginAttempts.Count, cleanupDate);
            }
        }

        private async Task CleanupResolvedSecurityIncidentsAsync(ApplicationDbContext context)
        {
            // Keep resolved security incidents for 2 years
            var cleanupDate = DateTimeOffset.UtcNow.AddYears(-2);
            
            var oldIncidents = await context.SecurityIncidents
                .Where(si => si.Status == "Resolved" && si.ResolvedAt < cleanupDate)
                .ToListAsync();

            if (oldIncidents.Any())
            {
                context.SecurityIncidents.RemoveRange(oldIncidents);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Cleaned up {Count} old resolved security incidents older than {Date}", 
                    oldIncidents.Count, cleanupDate);
            }
        }

        private async Task CleanupTemporaryFilesAsync()
        {
            try
            {
                var tempPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "temp");
                if (Directory.Exists(tempPath))
                {
                    var tempFiles = Directory.GetFiles(tempPath)
                        .Where(file => File.GetCreationTime(file) < DateTime.Now.AddDays(-1))
                        .ToList();

                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temporary file: {File}", file);
                        }
                    }

                    if (tempFiles.Any())
                    {
                        _logger.LogInformation("Cleaned up {Count} temporary files", tempFiles.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up temporary files");
            }
        }

        private async Task ArchiveDocumentFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var archivePath = filePath.Replace("wwwroot/uploads", "wwwroot/archive");
                    var archiveDir = Path.GetDirectoryName(archivePath);
                    
                    if (!string.IsNullOrEmpty(archiveDir) && !Directory.Exists(archiveDir))
                    {
                        Directory.CreateDirectory(archiveDir);
                    }

                    if (!string.IsNullOrEmpty(archivePath))
                    {
                        File.Move(filePath, archivePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to archive document file: {FilePath}", filePath);
            }
        }
    }
}