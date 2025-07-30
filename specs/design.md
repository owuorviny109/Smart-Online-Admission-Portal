# Smart Online Admission Portal (SOAP) - Design Document

## Overview

SOAP is a web-based platform that digitizes the post-NEMIS placement admission process for Kenyan secondary schools. The system consists of a parent-facing portal for form completion and document upload, a school administrator dashboard for application review, and SMS integration for parents with limited internet access.

## Architecture

### System Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Parent Portal │    │  Admin Dashboard│    │   SMS Gateway   │
│   (Next.js)     │    │   (Next.js)     │    │   (Africa's     │
│                 │    │                 │    │   Talking)      │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────┴───────────┐
                    │     API Server          │
                    │    (Node.js/Express)    │
                    └─────────────┬───────────┘
                                  │
                    ┌─────────────┴───────────┐
                    │     Database            │
                    │    (PostgreSQL)         │
                    └─────────────────────────┘
```

### ASP.NET MVC Project Structure (Modular Monolith)

```
SOAP.Web/                        # Main ASP.NET Core MVC Application
├── SOAP.Web.csproj
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── Startup.cs
│
├── Areas/                       # MVC Areas for organization
│   ├── Parent/                  # Parent portal area
│   │   ├── Controllers/
│   │   │   ├── HomeController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── ApplicationController.cs
│   │   │   └── DocumentController.cs
│   │   ├── Views/
│   │   │   ├── Shared/
│   │   │   │   ├── _Layout.cshtml
│   │   │   │   └── _ParentNav.cshtml
│   │   │   ├── Home/
│   │   │   ├── Auth/
│   │   │   ├── Application/
│   │   │   └── Document/
│   │   └── ViewModels/
│   │       ├── ApplicationViewModel.cs
│   │       └── DocumentUploadViewModel.cs
│   │
│   └── Admin/                   # School admin area
│       ├── Controllers/
│       │   ├── DashboardController.cs
│       │   ├── ApplicationController.cs
│       │   ├── StudentController.cs
│       │   └── AnalyticsController.cs
│       ├── Views/
│       │   ├── Shared/
│       │   │   ├── _AdminLayout.cshtml
│       │   │   └── _AdminNav.cshtml
│       │   ├── Dashboard/
│       │   ├── Application/
│       │   ├── Student/
│       │   └── Analytics/
│       └── ViewModels/
│           ├── DashboardViewModel.cs
│           └── ApplicationReviewViewModel.cs
│
├── Controllers/                 # Main controllers
│   ├── HomeController.cs
│   ├── AccountController.cs
│   └── ApiController.cs         # For AJAX/API endpoints
│
├── Models/                      # Entity Framework models
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── School.cs
│   │   ├── Application.cs
│   │   ├── Document.cs
│   │   └── SmsLog.cs
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   └── ApplicationViewModel.cs
│   └── DTOs/
│       ├── ApplicationDto.cs
│       └── DocumentDto.cs
│
├── Data/                        # Entity Framework context
│   ├── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── SchoolConfiguration.cs
│   │   └── ApplicationConfiguration.cs
│   ├── Migrations/
│   └── Seeds/
│       └── DefaultDataSeeder.cs
│
├── Services/                    # Business logic services
│   ├── Interfaces/
│   │   ├── IApplicationService.cs
│   │   ├── IDocumentService.cs
│   │   ├── ISmsService.cs
│   │   └── IEmailService.cs
│   ├── ApplicationService.cs
│   ├── DocumentService.cs
│   ├── SmsService.cs
│   ├── EmailService.cs
│   └── BackgroundServices/
│       └── SmsProcessingService.cs
│
├── Utilities/                   # Helper classes
│   ├── Extensions/
│   │   ├── StringExtensions.cs
│   │   └── ModelStateExtensions.cs
│   ├── Helpers/
│   │   ├── FileHelper.cs
│   │   ├── SmsHelper.cs
│   │   └── ValidationHelper.cs
│   └── Constants/
│       ├── ApplicationConstants.cs
│       └── MessageConstants.cs
│
├── Views/                       # Main views
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _LoginPartial.cshtml
│   │   ├── Error.cshtml
│   │   └── Components/
│   │       ├── Navigation/
│   │       └── Alerts/
│   ├── Home/
│   │   ├── Index.cshtml
│   │   └── About.cshtml
│   └── Account/
│       ├── Login.cshtml
│       ├── Register.cshtml
│       └── ForgotPassword.cshtml
│
├── wwwroot/                     # Static files
│   ├── css/
│   │   ├── bootstrap.min.css
│   │   ├── site.css
│   │   ├── parent.css
│   │   └── admin.css
│   ├── js/
│   │   ├── jquery.min.js
│   │   ├── bootstrap.bundle.min.js
│   │   ├── site.js
│   │   ├── parent.js
│   │   └── admin.js
│   ├── lib/                     # Third-party libraries
│   │   ├── datatables/
│   │   ├── sweetalert2/
│   │   └── fontawesome/
│   ├── images/
│   │   ├── logo.png
│   │   └── school-logos/
│   └── uploads/                 # File uploads
│       ├── documents/
│       └── temp/
│
├── Configuration/               # App configuration
│   ├── DatabaseConfig.cs
│   ├── SmsConfig.cs
│   └── FileStorageConfig.cs
│
└── Tests/                       # Unit and integration tests
    ├── SOAP.Web.Tests/
    │   ├── Controllers/
    │   ├── Services/
    │   └── Integration/
    └── SOAP.Web.IntegrationTests/
        ├── Areas/
        └── Api/
```
```

### Technology Stack

**Frontend & Backend:**
- ASP.NET Core 8 MVC (Single application with multiple areas)
- Entity Framework Core with SQL Server
- Bootstrap 5 + Custom CSS for responsive, modern UI
- jQuery for enhanced interactivity
- SignalR for real-time notifications

**Database:**
- SQL Server (LocalDB for development, SQL Server Express for production)
- Entity Framework Core Code-First approach
- SQL Server Management Studio for database management

**Authentication & Security:**
- ASP.NET Core Identity for user management
- Cookie-based authentication (simpler than JWT for MVC)
- Role-based authorization (Parent, Admin roles)

**File Handling:**
- Local file storage initially (wwwroot/uploads)
- IFormFile for file uploads with validation
- Future: Cloudinary or AWS S3 integration

**SMS Integration:**
- Africa's Talking SMS API for Kenya market
- Background services for SMS processing
- SMS webhook handling for delivery status

**UI Framework:**
- Bootstrap 5 for responsive design
- Font Awesome for icons
- Custom CSS for school branding
- DataTables for admin data grids
- SweetAlert2 for beautiful alerts

**Development & Deployment:**
- Visual Studio Community (free)
- SQL Server Express (free)
- IIS Express for local development
- Railway or DigitalOcean for hosting
- GitHub for version control

## Components and Interfaces

### 1. Parent Portal Components

**Authentication Module:**
- KCPE index number verification against school's uploaded student list
- Phone number OTP verification
- Session management with JWT tokens

**Application Form Module:**
- Student bio-data form with validation
- Parent/guardian information form
- Emergency contacts form
- Form progress tracking

**Document Upload Module:**
- File upload with drag-and-drop interface
- Document type validation (PDF/JPEG, max 2MB)
- Upload progress indicators
- Document preview functionality

**Status Dashboard:**
- Application completion status
- Document verification status
- Admission slip download
- SMS notifications history

### 2. School Administrator Dashboard

**Application Management:**
- Application list with filtering and search
- Application detail view with documents
- Approval/rejection workflow
- Bulk actions for multiple applications

**Document Review:**
- Document viewer with zoom/download
- Annotation tools for feedback
- Document status management
- Verification checklist

**Analytics Module:**
- Application statistics dashboard
- Daily progress reports
- Reporting day forecasts
- Export functionality

### 3. SMS Interface

**SMS Handler:**
- Incoming SMS parsing
- Multi-step form workflow via SMS
- Response validation and error handling
- Integration with main database

## Data Models

### User Model (Entity Framework)
```sql
CREATE TABLE [Users] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [PhoneNumber] NVARCHAR(15) UNIQUE NOT NULL,
  [Role] NVARCHAR(20) NOT NULL CHECK ([Role] IN ('Parent', 'Admin')),
  [SchoolId] INT NULL,
  [IsActive] BIT DEFAULT 1,
  [CreatedAt] DATETIME2 DEFAULT GETDATE(),
  [UpdatedAt] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([SchoolId]) REFERENCES [Schools]([Id])
);

-- Trigger for UpdatedAt
CREATE TRIGGER [TR_Users_UpdatedAt] ON [Users]
AFTER UPDATE AS
BEGIN
  UPDATE [Users] SET [UpdatedAt] = GETDATE()
  WHERE [Id] IN (SELECT [Id] FROM inserted)
END
```

### Student Application Model
```sql
CREATE TABLE [Applications] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [KcpeIndexNumber] NVARCHAR(20) UNIQUE NOT NULL,
  [StudentName] NVARCHAR(100) NOT NULL,
  [StudentAge] INT NOT NULL,
  [ParentPhone] NVARCHAR(15) NOT NULL,
  [ParentName] NVARCHAR(100) NOT NULL,
  [EmergencyContact] NVARCHAR(15) NOT NULL,
  [EmergencyName] NVARCHAR(100) NOT NULL,
  [HomeAddress] NVARCHAR(MAX),
  [BoardingStatus] NVARCHAR(20) DEFAULT 'Day' CHECK ([BoardingStatus] IN ('Boarding', 'Day')),
  [MedicalConditions] NVARCHAR(MAX),
  [SchoolId] INT NOT NULL,
  [Status] NVARCHAR(20) DEFAULT 'Pending' CHECK ([Status] IN ('Pending', 'Approved', 'Rejected', 'Incomplete')),
  [AdmissionCode] NVARCHAR(10) UNIQUE,
  [CheckedIn] BIT DEFAULT 0,
  [CreatedAt] DATETIME2 DEFAULT GETDATE(),
  [UpdatedAt] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([SchoolId]) REFERENCES [Schools]([Id]),
  INDEX [IX_Applications_SchoolStatus] ([SchoolId], [Status]),
  INDEX [IX_Applications_KcpeNumber] ([KcpeIndexNumber])
);
```

### School Students Model (Pre-loaded from Ministry List)
```sql
CREATE TABLE [SchoolStudents] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [KcpeIndexNumber] NVARCHAR(20) NOT NULL,
  [StudentName] NVARCHAR(100) NOT NULL,
  [KcpeScore] INT,
  [SchoolId] INT NOT NULL,
  [Year] INT NOT NULL,
  [HasApplied] BIT DEFAULT 0,
  [CreatedAt] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([SchoolId]) REFERENCES [Schools]([Id]),
  CONSTRAINT [UK_SchoolStudents_StudentSchoolYear] UNIQUE ([KcpeIndexNumber], [SchoolId], [Year]),
  INDEX [IX_SchoolStudents_SchoolYear] ([SchoolId], [Year])
);
```

### Document Model
```sql
CREATE TABLE [Documents] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [ApplicationId] INT NOT NULL,
  [DocumentType] NVARCHAR(30) NOT NULL CHECK ([DocumentType] IN ('KcpeSlip', 'BirthCertificate', 'MedicalForm')),
  [FileName] NVARCHAR(255) NOT NULL,
  [FilePath] NVARCHAR(500) NOT NULL,
  [FileSize] BIGINT NOT NULL,
  [ContentType] NVARCHAR(100) NOT NULL,
  [UploadStatus] NVARCHAR(20) DEFAULT 'Uploaded' CHECK ([UploadStatus] IN ('Uploaded', 'Verified', 'Rejected')),
  [AdminFeedback] NVARCHAR(MAX),
  [CreatedAt] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([ApplicationId]) REFERENCES [Applications]([Id]) ON DELETE CASCADE,
  INDEX [IX_Documents_ApplicationType] ([ApplicationId], [DocumentType])
);
```

### School Model
```sql
CREATE TABLE [Schools] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [Name] NVARCHAR(200) NOT NULL,
  [Code] NVARCHAR(10) UNIQUE NOT NULL,
  [County] NVARCHAR(50) NOT NULL,
  [ContactPhone] NVARCHAR(15),
  [ContactEmail] NVARCHAR(100),
  [LogoPath] NVARCHAR(500),
  [IsActive] BIT DEFAULT 1,
  [CreatedAt] DATETIME2 DEFAULT GETDATE(),
  INDEX [IX_Schools_County] ([County]),
  INDEX [IX_Schools_Code] ([Code])
);
```

### SMS Log Model
```sql
CREATE TABLE [SmsLogs] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [PhoneNumber] NVARCHAR(15) NOT NULL,
  [MessageType] NVARCHAR(20) NOT NULL CHECK ([MessageType] IN ('Incoming', 'Outgoing')),
  [Content] NVARCHAR(MAX) NOT NULL,
  [Status] NVARCHAR(20) DEFAULT 'Sent' CHECK ([Status] IN ('Sent', 'Delivered', 'Failed')),
  [ApplicationId] INT,
  [CreatedAt] DATETIME2 DEFAULT GETDATE(),
  FOREIGN KEY ([ApplicationId]) REFERENCES [Applications]([Id]),
  INDEX [IX_SmsLogs_PhoneDate] ([PhoneNumber], [CreatedAt]),
  INDEX [IX_SmsLogs_Status] ([Status])
);
```

## Error Handling

### Frontend Error Handling
- Form validation with real-time feedback
- File upload error handling with retry mechanism
- Network error handling with offline indicators
- User-friendly error messages in English and Swahili

### Backend Error Handling
- Input validation using Joi or Zod
- Database error handling with transaction rollbacks
- File upload error handling with cleanup
- SMS API error handling with retry logic
- Comprehensive logging with Winston

### SMS Error Handling
- Invalid NEMIS number responses
- Incomplete form submission handling
- Network timeout handling
- Fallback to web portal instructions

## UI/UX Design Layout

### Design System & Visual Identity

**Color Palette:**
```
Primary Colors:
- Education Blue: #1e40af (Trust, professionalism)
- Success Green: #059669 (Approval, completion)
- Warning Orange: #d97706 (Pending, attention needed)
- Error Red: #dc2626 (Rejection, errors)

Secondary Colors:
- Light Gray: #f8fafc (Backgrounds)
- Medium Gray: #64748b (Text secondary)
- Dark Gray: #1e293b (Text primary)
- White: #ffffff (Cards, modals)
```

**Typography:**
```
Headings: Inter (Clean, modern, readable)
Body Text: Inter (Consistent with headings)
Code/Numbers: JetBrains Mono (KCPE numbers, codes)
```

### Parent Portal Design Layout

**Landing Page Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ [SCHOOL LOGO]    SOAP - Smart Admission Portal    [LOGIN] │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│           Welcome to [School Name] Admission Portal        │
│                                                             │
│    ┌─────────────────────────────────────────────────┐    │
│    │  📱 Enter Your Child's KCPE Index Number        │    │
│    │  ┌─────────────────────────────────────────┐    │    │
│    │  │ KCPE Index: [________________] [VERIFY] │    │    │
│    │  └─────────────────────────────────────────┘    │    │
│    │                                                 │    │
│    │  ✓ Complete application online                  │    │
│    │  ✓ Upload documents securely                    │    │
│    │  ✓ Track application status                     │    │
│    │  ✓ Receive SMS notifications                    │    │
│    └─────────────────────────────────────────────────┘    │
│                                                             │
│  Need Help? Call: 0700-XXX-XXX | SMS: 40404               │
└─────────────────────────────────────────────────────────────┘
```

**Application Form Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ [SCHOOL LOGO] Student Application Form           [LOGOUT]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Progress: [████████░░] 80% Complete                        │
│                                                             │
│ ┌─── Student Information ─────────────────────────────────┐ │
│ │ Full Name: [John Doe Mwangi        ] (from KCPE)      │ │
│ │ Age: [14] years                                        │ │
│ │ KCPE Score: [350] (from KCPE records)                 │ │
│ │ Boarding: ○ Boarding ● Day Scholar                    │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Parent/Guardian Information ─────────────────────────┐ │
│ │ Parent Name: [Mary Wanjiku Mwangi  ]                  │ │
│ │ Phone: [0722-XXX-XXX] [VERIFY]                        │ │
│ │ Email: [mary@email.com] (optional)                    │ │
│ │ Relationship: [Mother ▼]                              │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Emergency Contact ───────────────────────────────────┐ │
│ │ Name: [Peter Mwangi    ]                              │ │
│ │ Phone: [0733-XXX-XXX]                                 │ │
│ │ Relationship: [Father ▼]                              │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│              [SAVE DRAFT]    [CONTINUE →]                   │
└─────────────────────────────────────────────────────────────┘
```

**Document Upload Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ [SCHOOL LOGO] Document Upload                    [LOGOUT]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ Progress: [██████████] 100% Complete                       │
│                                                             │
│ ┌─── Required Documents ──────────────────────────────────┐ │
│ │                                                         │ │
│ │ 1. KCPE Result Slip                          ✓ UPLOADED │ │
│ │    ┌─────────────────────────────────────────────────┐  │ │
│ │    │ 📄 kcpe_result_john_doe.pdf (1.2 MB)          │  │ │
│ │    │ [VIEW] [REPLACE]                               │  │ │
│ │    └─────────────────────────────────────────────────┘  │ │
│ │                                                         │ │
│ │ 2. Birth Certificate                         ⏳ PENDING │ │
│ │    ┌─────────────────────────────────────────────────┐  │ │
│ │    │ 📤 Drag & drop or [BROWSE FILES]              │  │ │
│ │    │ Accepted: PDF, JPEG (Max 2MB)                 │  │ │
│ │    └─────────────────────────────────────────────────┘  │ │
│ │                                                         │ │
│ │ 3. Medical Form                              ❌ MISSING │ │
│ │    ┌─────────────────────────────────────────────────┐  │ │
│ │    │ 📤 Drag & drop or [BROWSE FILES]              │  │ │
│ │    │ Accepted: PDF, JPEG (Max 2MB)                 │  │ │
│ │    └─────────────────────────────────────────────────┘  │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│              [← BACK]    [SUBMIT APPLICATION]               │
└─────────────────────────────────────────────────────────────┘
```

**Status Dashboard Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ [SCHOOL LOGO] Application Status                 [LOGOUT]   │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─── Application Summary ─────────────────────────────────┐ │
│ │ Student: John Doe Mwangi                               │ │
│ │ KCPE Index: 12345678901                               │ │
│ │ School: Alliance High School                           │ │
│ │ Status: 🟡 UNDER REVIEW                               │ │
│ │ Submitted: Jan 15, 2024 at 2:30 PM                   │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Document Status ─────────────────────────────────────┐ │
│ │ ✅ KCPE Result Slip - Verified                        │ │
│ │ ⏳ Birth Certificate - Under Review                   │ │
│ │ ❌ Medical Form - Rejected                            │ │
│ │    💬 "Please upload clearer image" - Admin           │ │
│ │    [UPLOAD NEW FILE]                                   │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Next Steps ──────────────────────────────────────────┐ │
│ │ 📋 Upload clearer medical form                        │ │
│ │ ⏰ Wait for document review (1-2 days)                │ │
│ │ 📱 You'll receive SMS when status changes             │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│              [EDIT APPLICATION]    [DOWNLOAD RECEIPT]       │
└─────────────────────────────────────────────────────────────┘
```

### Admin Dashboard Design Layout

**Admin Dashboard Overview:**
```
┌─────────────────────────────────────────────────────────────┐
│ [SCHOOL LOGO] Admin Dashboard                    [LOGOUT]   │
├─────────────────────────────────────────────────────────────┤
│ [Dashboard] [Applications] [Students] [Analytics] [Settings]│
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ ┌─── Quick Stats ─────────────────────────────────────────┐ │
│ │ 📊 Total Applications: 1,247                          │ │
│ │ ✅ Approved: 892     ⏳ Pending: 298    ❌ Rejected: 57│ │
│ │ 📈 Today: +23 new applications                        │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Recent Applications ─────────────────────────────────┐ │
│ │ Search: [________________] [🔍] Filter: [All ▼]       │ │
│ │                                                         │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │ Name          KCPE Index    Status      Actions     │ │ │
│ │ ├─────────────────────────────────────────────────────┤ │ │
│ │ │ John Mwangi   12345678901   🟡 Pending  [REVIEW]   │ │ │
│ │ │ Mary Wanjiku  12345678902   ✅ Approved [VIEW]     │ │ │
│ │ │ Peter Kiprotich 12345678903 ❌ Rejected [EDIT]     │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ │                                                         │ │
│ │ Showing 1-10 of 298 pending    [← Previous] [Next →]  │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**Application Review Layout:**
```
┌─────────────────────────────────────────────────────────────┐
│ [SCHOOL LOGO] Review Application                 [LOGOUT]   │
├─────────────────────────────────────────────────────────────┤
│ [← Back to Dashboard]                                       │
│                                                             │
│ ┌─── Student Information ─────────────────────────────────┐ │
│ │ Name: John Doe Mwangi                                  │ │
│ │ KCPE Index: 12345678901    Score: 350                 │ │
│ │ Age: 14 years             Boarding: Day Scholar        │ │
│ │ Applied: Jan 15, 2024 at 2:30 PM                      │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Documents Review ────────────────────────────────────┐ │
│ │ 1. KCPE Result Slip                    [VIEW] [APPROVE] │ │
│ │    📄 kcpe_result_john_doe.pdf (1.2 MB)               │ │
│ │                                                         │ │
│ │ 2. Birth Certificate                   [VIEW] [APPROVE] │ │
│ │    📄 birth_cert_john_doe.pdf (800 KB)                │ │
│ │                                                         │ │
│ │ 3. Medical Form                        [VIEW] [REJECT]  │ │
│ │    📄 medical_form_john_doe.jpg (2.1 MB)              │ │
│ │    💬 Feedback: [Image too blurry, please re-upload]   │ │
│ └───────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─── Decision ────────────────────────────────────────────┐ │
│ │ ○ Approve Application                                   │ │
│ │ ● Request Document Corrections                          │ │
│ │ ○ Reject Application                                    │ │
│ │                                                         │ │
│ │ Comments: [Please upload a clearer medical form]       │ │
│ │                                                         │ │
│ │ [SAVE DECISION]    [SEND SMS NOTIFICATION]             │ │
│ └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Mobile-Responsive Design

**Mobile Parent Portal (320px width):**
```
┌─────────────────────┐
│ [☰] SOAP      [👤] │
├─────────────────────┤
│                     │
│   Welcome to        │
│ Alliance High School│
│                     │
│ ┌─────────────────┐ │
│ │ KCPE Index:     │ │
│ │ [_____________] │ │
│ │ [VERIFY]        │ │
│ └─────────────────┘ │
│                     │
│ ✓ Apply online      │
│ ✓ Upload docs       │
│ ✓ Track status      │
│ ✓ Get SMS updates   │
│                     │
│ Need help?          │
│ Call: 0700-XXX-XXX  │
└─────────────────────┘
```

### Design Principles

**User Experience:**
- **Progressive Disclosure:** Show information step-by-step
- **Clear Visual Hierarchy:** Important actions stand out
- **Consistent Patterns:** Same interactions work the same way
- **Error Prevention:** Validate inputs before submission
- **Accessibility:** Screen reader friendly, keyboard navigation

**Visual Design:**
- **Clean & Modern:** Minimal clutter, plenty of white space
- **School Branding:** Customizable colors and logos per school
- **Status Indicators:** Clear visual feedback for all states
- **Mobile-First:** Responsive design that works on all devices

**Performance:**
- **Fast Loading:** Optimized images and minimal JavaScript
- **Offline Support:** Basic functionality works without internet
- **Progressive Enhancement:** Works without JavaScript enabled

## Testing Strategy

### Unit Testing
- Backend API endpoints testing with Jest
- Frontend component testing with React Testing Library
- Database model testing with test database
- SMS handler testing with mock SMS gateway

### Integration Testing
- End-to-end application flow testing
- File upload and storage testing
- SMS workflow testing
- Database integration testing

### User Acceptance Testing
- Parent portal usability testing
- Admin dashboard workflow testing
- SMS interface testing with real users
- Mobile responsiveness testing

### Performance Testing
- Load testing for concurrent users
- File upload performance testing
- Database query optimization testing
- SMS gateway throughput testing

## Security Considerations

### Data Protection
- HTTPS encryption for all communications
- JWT token-based authentication
- Phone number verification via OTP
- File upload validation and scanning

### Privacy Compliance
- Kenya Data Protection Act compliance
- Minimal data collection principle
- Data retention policies
- User consent management

### Access Control
- Role-based access control (parent vs admin)
- School-specific data isolation
- Document access restrictions
- Admin action audit trails

## Scalability & Maintainability Principles

### 1. Microservices Architecture
- **Independent Services:** Each service can be developed, deployed, and scaled independently
- **Service Communication:** REST APIs with potential future migration to GraphQL
- **Database Per Service:** Each service owns its data (Database-per-service pattern)
- **Event-Driven Architecture:** Services communicate via events for loose coupling

### 2. Monorepo Benefits
- **Shared Code:** Common types, UI components, and utilities shared across services
- **Atomic Changes:** Cross-service changes can be made in single commits
- **Consistent Tooling:** Same linting, testing, and build tools across all projects
- **Dependency Management:** Centralized dependency management with workspace support

### 3. Future Expansion Ready
- **Plugin Architecture:** New features can be added as separate services
- **Multi-tenancy:** Architecture supports multiple schools/districts
- **API Versioning:** Built-in API versioning for backward compatibility
- **Feature Flags:** Toggle features without deployments

### 4. Development Workflow
- **Hot Module Replacement:** Fast development with instant feedback
- **Automated Testing:** Unit, integration, and E2E tests for all services
- **CI/CD Pipeline:** Automated testing, building, and deployment
- **Code Generation:** Automated generation of boilerplate code

### 5. Monitoring & Observability
- **Distributed Tracing:** Track requests across services
- **Centralized Logging:** All service logs in one place
- **Health Checks:** Monitor service health and dependencies
- **Performance Metrics:** Track response times, throughput, and errors

### 6. Future Components (Roadmap)
```
Year 1 Additions:
├── payment-service/              # Fee payment integration
├── exam-service/                 # Exam management
└── timetable-service/            # Class scheduling

Year 2 Additions:
├── library-service/              # Library management
├── transport-service/            # School transport
├── hostel-service/               # Boarding management
└── parent-communication/         # Parent-teacher communication

Year 3 Additions:
├── hr-service/                   # Staff management
├── inventory-service/            # School inventory
├── finance-service/              # Financial management
└── mobile-apps/                  # Native mobile applications
```

## Deployment Strategy

### Development Environment
- Local development with Docker containers
- Test database with sample data
- Mock SMS gateway for testing
- Hot reload for rapid development

### Staging Environment
- Production-like environment for testing
- Real SMS gateway integration testing
- Performance testing environment
- User acceptance testing platform

### Production Environment
- Scalable cloud deployment
- Database backups and monitoring
- SMS gateway redundancy
- Error monitoring and alerting

## Integration Points

### School Data Management
- School admin uploads NEMIS placement list (CSV/Excel)
- KCPE index number validation against school's student list
- Student pre-population from uploaded placement data

### SMS Gateway Integration
- Africa's Talking API integration
- Two-way SMS communication
- Delivery status tracking
- Cost optimization strategies

### File Storage Integration
- AWS S3 or DigitalOcean Spaces
- CDN for fast document access
- Backup and disaster recovery
- Cost-effective storage tiers