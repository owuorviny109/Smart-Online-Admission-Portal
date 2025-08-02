using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOAP.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProcessingConsents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConsentGiven = table.Column<bool>(type: "bit", nullable: false),
                    ConsentVersion = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConsentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    WithdrawnDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingConsents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    County = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Subdomain = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ResourceAccessed = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActionPerformed = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AffectedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SourceIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    AutomaticResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ManualResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityIncidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KcpeIndexNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "KCPE index number for student verification"),
                    StudentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Student full name"),
                    StudentAge = table.Column<int>(type: "int", nullable: false),
                    ParentPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, comment: "Encrypted personal data"),
                    ParentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Parent/guardian full name"),
                    EmergencyContact = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, comment: "Encrypted emergency contact phone"),
                    EmergencyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Emergency contact name"),
                    HomeAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Encrypted personal data"),
                    BoardingStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Day", comment: "Boarding or Day scholar preference"),
                    MedicalConditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, comment: "Encrypted personal data"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending", comment: "Application status (Pending, Approved, Rejected)"),
                    AdmissionCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, comment: "Unique admission code for approved applications"),
                    CheckedIn = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Whether student has physically checked in"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Application creation timestamp"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Last update timestamp"),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When application was submitted for review"),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When application was reviewed by admin"),
                    ReviewedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Admin user who reviewed the application")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Row-level security: Users can only access applications from their school");

            migrationBuilder.CreateTable(
                name: "SchoolStudents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolId = table.Column<int>(type: "int", nullable: false),
                    KcpeIndexNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "KCPE index number for student identification"),
                    StudentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "Student full name from KCPE records"),
                    KcpeScore = table.Column<int>(type: "int", nullable: false, comment: "KCPE total score"),
                    Year = table.Column<int>(type: "int", nullable: false, comment: "KCPE examination year"),
                    PlacementStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Placed", comment: "Student placement status (Placed, NotPlaced, Transferred)"),
                    HasApplied = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Record creation timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolStudents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolStudents_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Pre-loaded student records for KCPE verification and placement tracking");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, comment: "Encrypted phone number for authentication"),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "User role (Parent, SchoolAdmin, SuperAdmin)"),
                    SchoolId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true, comment: "Whether the user account is active"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Account creation timestamp"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Last successful login timestamp"),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Number of consecutive failed login attempts"),
                    LockedUntil = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Account lockout expiry time"),
                    DeletionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Date when user data was anonymized/deleted"),
                    DeletionReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Reason for data deletion (GDPR compliance)"),
                    SchoolId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolId1",
                        column: x => x.SchoolId1,
                        principalTable: "Schools",
                        principalColumn: "Id");
                },
                comment: "User accounts with role-based access control and security monitoring");

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Type of document (BIRTH_CERT, KCPE_CERT, etc.)"),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Original filename as uploaded by user"),
                    SecureFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Secure filename used for storage"),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false, comment: "File size in bytes"),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "MIME type of the document"),
                    FileHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "SHA-256 hash of file content for integrity verification"),
                    VerificationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending", comment: "Document verification status (Pending, Verified, Rejected)"),
                    UploadStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VerifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Admin user who verified the document"),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When document was verified"),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true, comment: "Reason for document rejection"),
                    IsVirusScanPassed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Whether document passed virus scan"),
                    VirusScanDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When virus scan was performed"),
                    AccessLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Private", comment: "Access level (Private, SchoolAdmin, Public)"),
                    EncryptionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Encryption key reference for sensitive documents"),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "When document was uploaded"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Last update timestamp"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AdminFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Row-level security: Users can only access documents from applications they own or manage");

            migrationBuilder.CreateTable(
                name: "SmsLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, comment: "Recipient phone number"),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, comment: "Type of SMS (OTP, NOTIFICATION, ALERT)"),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false, comment: "SMS message content"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending", comment: "SMS delivery status (Pending, Sent, Failed, Delivered)"),
                    ApplicationId = table.Column<int>(type: "int", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When SMS was sent to provider"),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When SMS was delivered to recipient"),
                    FailureReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true, comment: "Reason for SMS failure"),
                    ProviderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "SMS provider message ID"),
                    Cost = table.Column<decimal>(type: "decimal(10,4)", nullable: true, comment: "Cost of SMS in local currency"),
                    RetryCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Number of retry attempts"),
                    MaxRetries = table.Column<int>(type: "int", nullable: false, defaultValue: 3, comment: "Maximum retry attempts allowed"),
                    NextRetryAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "When next retry should be attempted"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "When SMS was queued"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()", comment: "Last update timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsLogs_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                },
                comment: "SMS communication log with delivery tracking and rate limiting");

            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    OtpCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OtpExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OtpUsed = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CreatedAt",
                table: "Applications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_KcpeNumber",
                table: "Applications",
                column: "KcpeIndexNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ParentPhone_School",
                table: "Applications",
                columns: new[] { "ParentPhone", "SchoolId" },
                filter: "ParentPhone IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ReviewedBy",
                table: "Applications",
                column: "ReviewedBy",
                filter: "ReviewedBy IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_School_Status",
                table: "Applications",
                columns: new[] { "SchoolId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_School_Status_Date",
                table: "Applications",
                columns: new[] { "SchoolId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_Status_Updated",
                table: "Applications",
                columns: new[] { "Status", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingConsents_ConsentDate",
                table: "DataProcessingConsents",
                column: "ConsentDate");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingConsents_Type_Active",
                table: "DataProcessingConsents",
                columns: new[] { "ConsentType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingConsents_User_Type",
                table: "DataProcessingConsents",
                columns: new[] { "UserId", "ConsentType" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Application_Access_Date",
                table: "Documents",
                columns: new[] { "ApplicationId", "AccessLevel", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Application_Type",
                table: "Documents",
                columns: new[] { "ApplicationId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_FileHash",
                table: "Documents",
                column: "FileHash",
                filter: "FileHash IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Status_Uploaded",
                table: "Documents",
                columns: new[] { "VerificationStatus", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VerifiedBy",
                table: "Documents",
                column: "VerifiedBy",
                filter: "VerifiedBy IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VirusScan",
                table: "Documents",
                columns: new[] { "IsVirusScanPassed", "VirusScanDate" },
                filter: "IsVirusScanPassed = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IP_Date",
                table: "LoginAttempts",
                columns: new[] { "IpAddress", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Phone_Date",
                table: "LoginAttempts",
                columns: new[] { "PhoneNumber", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Phone_Success_Date",
                table: "LoginAttempts",
                columns: new[] { "PhoneNumber", "Success", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Success_Date",
                table: "LoginAttempts",
                columns: new[] { "Success", "AttemptedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_UserId",
                table: "LoginAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_Code",
                table: "Schools",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_County",
                table: "Schools",
                column: "County");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_County_Active",
                table: "Schools",
                columns: new[] { "County", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolStudents_KcpeNumber_School_Year",
                table: "SchoolStudents",
                columns: new[] { "KcpeIndexNumber", "SchoolId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolStudents_School_Status",
                table: "SchoolStudents",
                columns: new[] { "SchoolId", "PlacementStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolStudents_School_Year",
                table: "SchoolStudents",
                columns: new[] { "SchoolId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolStudents_Score",
                table: "SchoolStudents",
                column: "KcpeScore");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_EventType_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "EventType", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_IpAddress_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "IpAddress", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Success",
                table: "SecurityAuditLogs",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UserId_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_AffectedUser",
                table: "SecurityIncidents",
                column: "AffectedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_IP_Date",
                table: "SecurityIncidents",
                columns: new[] { "SourceIpAddress", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Status_Severity",
                table: "SecurityIncidents",
                columns: new[] { "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityIncidents_Type_Date",
                table: "SecurityIncidents",
                columns: new[] { "IncidentType", "DetectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_ApplicationId",
                table: "SmsLogs",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Phone_Date",
                table: "SmsLogs",
                columns: new[] { "PhoneNumber", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Phone_Type_Date",
                table: "SmsLogs",
                columns: new[] { "PhoneNumber", "MessageType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Status",
                table: "SmsLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Status_Date_Cost",
                table: "SmsLogs",
                columns: new[] { "Status", "CreatedAt", "Cost" },
                filter: "Status = 'Delivered' AND Cost IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Status_NextRetry",
                table: "SmsLogs",
                columns: new[] { "Status", "NextRetryAt" },
                filter: "Status = 'Failed' AND NextRetryAt IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Type_Status_Date",
                table: "SmsLogs",
                columns: new[] { "MessageType", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Active_LastLogin",
                table: "Users",
                columns: new[] { "IsActive", "LastLoginAt" },
                filter: "IsActive = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LockedUntil",
                table: "Users",
                column: "LockedUntil",
                filter: "LockedUntil IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_Active",
                table: "Users",
                columns: new[] { "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_School_Role",
                table: "Users",
                columns: new[] { "SchoolId", "Role" },
                filter: "SchoolId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolId1",
                table: "Users",
                column: "SchoolId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProcessingConsents");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "SchoolStudents");

            migrationBuilder.DropTable(
                name: "SecurityAuditLogs");

            migrationBuilder.DropTable(
                name: "SecurityIncidents");

            migrationBuilder.DropTable(
                name: "SmsLogs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Schools");
        }
    }
}
