using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Utilities.Constants;
using Xunit;

namespace SOAP.Web.Tests.Services
{
    public class DataFilterServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<DataFilterService>> _mockLogger;
        private readonly Mock<IRoleValidationService> _mockRoleValidationService;
        private readonly DataFilterService _service;

        // Test users
        private readonly User _platformAdmin;
        private readonly User _schoolAdmin;
        private readonly User _parent;
        private readonly User _fakeAdmin;

        public DataFilterServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<DataFilterService>>();
            _mockRoleValidationService = new Mock<IRoleValidationService>();
            _service = new DataFilterService(_context, _mockLogger.Object, _mockRoleValidationService.Object);

            // Create test users
            _platformAdmin = new User
            {
                Id = 1,
                PhoneNumber = UserRoles.PLATFORM_ADMIN_PHONES[0],
                Role = UserRoles.PlatformAdmin,
                IsActive = true
            };

            _schoolAdmin = new User
            {
                Id = 2,
                PhoneNumber = "+254700000002",
                Role = UserRoles.SchoolAdmin,
                SchoolId = 1,
                IsActive = true
            };

            _parent = new User
            {
                Id = 3,
                PhoneNumber = "+254700000003",
                Role = UserRoles.Parent,
                IsActive = true
            };

            _fakeAdmin = new User
            {
                Id = 4,
                PhoneNumber = "+254700000004", // Not in authorized list
                Role = UserRoles.PlatformAdmin,
                IsActive = true
            };

            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add schools
            var school1 = new School { Id = 1, Name = "Test High School 1", Code = "THS001", County = "Test County", IsActive = true };
            var school2 = new School { Id = 2, Name = "Test High School 2", Code = "THS002", County = "Test County", IsActive = true };
            _context.Schools.AddRange(school1, school2);

            // Add users
            _context.Users.AddRange(_platformAdmin, _schoolAdmin, _parent, _fakeAdmin);

            // Add applications
            var app1 = new Application
            {
                Id = 1,
                KcpeIndexNumber = "12345678",
                StudentName = "John Doe",
                ParentPhone = _parent.PhoneNumber,
                ParentName = "Jane Doe",
                SchoolId = 1,
                Status = "Pending"
            };

            var app2 = new Application
            {
                Id = 2,
                KcpeIndexNumber = "87654321",
                StudentName = "Alice Smith",
                ParentPhone = "+254700000005",
                ParentName = "Bob Smith",
                SchoolId = 2,
                Status = "Approved"
            };

            _context.Applications.AddRange(app1, app2);

            // Add documents
            var doc1 = new Document
            {
                Id = 1,
                ApplicationId = 1,
                DocumentType = "KcpeSlip",
                FileName = "kcpe_slip.pdf",
                FilePath = "/documents/kcpe_slip.pdf",
                UploadStatus = "Uploaded"
            };

            var doc2 = new Document
            {
                Id = 2,
                ApplicationId = 2,
                DocumentType = "BirthCertificate",
                FileName = "birth_cert.pdf",
                FilePath = "/documents/birth_cert.pdf",
                UploadStatus = "Verified"
            };

            _context.Documents.AddRange(doc1, doc2);

            _context.SaveChanges();
        }

        [Fact]
        public void FilterApplications_PlatformAdmin_ReturnsAllApplications()
        {
            // Arrange
            var query = _context.Applications.AsQueryable();

            // Act
            var result = _service.FilterApplications(query, _platformAdmin).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterApplications_FakePlatformAdmin_ReturnsEmpty()
        {
            // Arrange
            var query = _context.Applications.AsQueryable();

            // Act
            var result = _service.FilterApplications(query, _fakeAdmin).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void FilterApplications_SchoolAdmin_ReturnsOnlyTheirSchoolApplications()
        {
            // Arrange
            var query = _context.Applications.AsQueryable();

            // Act
            var result = _service.FilterApplications(query, _schoolAdmin).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].SchoolId);
        }

        [Fact]
        public void FilterApplications_Parent_ReturnsOnlyTheirApplications()
        {
            // Arrange
            var query = _context.Applications.AsQueryable();

            // Act
            var result = _service.FilterApplications(query, _parent).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(_parent.PhoneNumber, result[0].ParentPhone);
        }

        [Fact]
        public void FilterDocuments_SchoolAdmin_ReturnsOnlyTheirSchoolDocuments()
        {
            // Arrange
            var query = _context.Documents.Include(d => d.Application).AsQueryable();

            // Act
            var result = _service.FilterDocuments(query, _schoolAdmin).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Application.SchoolId);
        }

        [Fact]
        public void FilterDocuments_Parent_ReturnsOnlyTheirDocuments()
        {
            // Arrange
            var query = _context.Documents.Include(d => d.Application).AsQueryable();

            // Act
            var result = _service.FilterDocuments(query, _parent).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(_parent.PhoneNumber, result[0].Application.ParentPhone);
        }

        [Fact]
        public void FilterSchools_Parent_ReturnsEmpty()
        {
            // Arrange
            var query = _context.Schools.AsQueryable();

            // Act
            var result = _service.FilterSchools(query, _parent).ToList();

            // Assert
            Assert.Empty(result); // Parents cannot access school data
        }

        [Fact]
        public void FilterSchools_SchoolAdmin_ReturnsOnlyTheirSchool()
        {
            // Arrange
            var query = _context.Schools.AsQueryable();

            // Act
            var result = _service.FilterSchools(query, _schoolAdmin).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(_schoolAdmin.SchoolId, result[0].Id);
        }

        [Fact]
        public void FilterUsers_Parent_ReturnsOnlyThemselves()
        {
            // Arrange
            var query = _context.Users.AsQueryable();

            // Act
            var result = _service.FilterUsers(query, _parent).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(_parent.Id, result[0].Id);
        }

        [Fact]
        public async Task CanAccessEntityAsync_Application_ValidatesCorrectly()
        {
            // Act & Assert
            Assert.True(await _service.CanAccessEntityAsync<Application>(_platformAdmin, 1));
            Assert.True(await _service.CanAccessEntityAsync<Application>(_schoolAdmin, 1));
            Assert.False(await _service.CanAccessEntityAsync<Application>(_schoolAdmin, 2)); // Different school
            Assert.True(await _service.CanAccessEntityAsync<Application>(_parent, 1));
            Assert.False(await _service.CanAccessEntityAsync<Application>(_parent, 2)); // Different parent
            Assert.False(await _service.CanAccessEntityAsync<Application>(_fakeAdmin, 1)); // Fake admin
        }

        [Fact]
        public async Task CanAccessEntityAsync_Document_ValidatesCorrectly()
        {
            // Act & Assert
            Assert.True(await _service.CanAccessEntityAsync<Document>(_platformAdmin, 1));
            Assert.True(await _service.CanAccessEntityAsync<Document>(_schoolAdmin, 1));
            Assert.False(await _service.CanAccessEntityAsync<Document>(_schoolAdmin, 2)); // Different school
            Assert.True(await _service.CanAccessEntityAsync<Document>(_parent, 1));
            Assert.False(await _service.CanAccessEntityAsync<Document>(_parent, 2)); // Different parent
        }

        [Fact]
        public async Task CanAccessEntityAsync_School_ValidatesCorrectly()
        {
            // Act & Assert
            Assert.True(await _service.CanAccessEntityAsync<School>(_platformAdmin, 1));
            Assert.True(await _service.CanAccessEntityAsync<School>(_schoolAdmin, 1));
            Assert.False(await _service.CanAccessEntityAsync<School>(_schoolAdmin, 2)); // Different school
            Assert.False(await _service.CanAccessEntityAsync<School>(_parent, 1)); // Parents cannot access schools
        }

        [Fact]
        public void GetUserDataScope_PlatformAdmin_ReturnsFullAccess()
        {
            // Act
            var scope = _service.GetUserDataScope(_platformAdmin);

            // Assert
            Assert.True(scope.CanAccessAllSchools);
            Assert.True(scope.CanAccessAllUsers);
            Assert.True(scope.CanAccessSystemLogs);
            Assert.True(scope.CanAccessBillingData);
            Assert.Contains("MANAGE", scope.AllowedOperations);
        }

        [Fact]
        public void GetUserDataScope_FakePlatformAdmin_ReturnsNoAccess()
        {
            // Act
            var scope = _service.GetUserDataScope(_fakeAdmin);

            // Assert
            Assert.False(scope.CanAccessAllSchools);
            Assert.False(scope.CanAccessAllUsers);
            Assert.False(scope.CanAccessSystemLogs);
            Assert.False(scope.CanAccessBillingData);
            Assert.Empty(scope.AllowedOperations);
        }

        [Fact]
        public void GetUserDataScope_SchoolAdmin_ReturnsSchoolRestrictedAccess()
        {
            // Act
            var scope = _service.GetUserDataScope(_schoolAdmin);

            // Assert
            Assert.False(scope.CanAccessAllSchools);
            Assert.Equal(_schoolAdmin.SchoolId, scope.RestrictedToSchoolId);
            Assert.Contains("CREATE", scope.AllowedOperations);
            Assert.Contains("READ", scope.AllowedOperations);
            Assert.DoesNotContain("DELETE", scope.AllowedOperations);
        }

        [Fact]
        public void GetUserDataScope_Parent_ReturnsMinimalAccess()
        {
            // Act
            var scope = _service.GetUserDataScope(_parent);

            // Assert
            Assert.False(scope.CanAccessAllSchools);
            Assert.Equal(_parent.PhoneNumber, scope.RestrictedToPhoneNumber);
            Assert.Contains("READ", scope.AllowedOperations);
            Assert.Contains("UPDATE", scope.AllowedOperations);
            Assert.DoesNotContain("DELETE", scope.AllowedOperations);
            Assert.DoesNotContain("CREATE", scope.AllowedOperations);
        }

        [Fact]
        public async Task ValidateAndLogDataAccessAsync_ValidOperation_ReturnsTrue()
        {
            // Act
            var result = await _service.ValidateAndLogDataAccessAsync<Application>(_schoolAdmin, "READ", 1);

            // Assert
            Assert.True(result);
            
            // Verify security log was created
            var securityLog = await _context.SecurityAuditLogs.FirstOrDefaultAsync();
            Assert.NotNull(securityLog);
            Assert.Equal("DATA_ACCESS_READ", securityLog.EventType);
            Assert.True(securityLog.Success);
        }

        [Fact]
        public async Task ValidateAndLogDataAccessAsync_InvalidOperation_ReturnsFalse()
        {
            // Act
            var result = await _service.ValidateAndLogDataAccessAsync<Application>(_parent, "DELETE", 1);

            // Assert
            Assert.False(result); // Parents cannot DELETE
        }

        [Fact]
        public void ApplyUserFilter_InactiveUser_ReturnsEmpty()
        {
            // Arrange
            var inactiveUser = new User { Id = 99, IsActive = false, Role = UserRoles.Parent };
            var query = _context.Applications.AsQueryable();

            // Act
            var result = _service.ApplyUserFilter(query, inactiveUser).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ApplyUserFilter_NullUser_ReturnsEmpty()
        {
            // Arrange
            var query = _context.Applications.AsQueryable();

            // Act
            var result = _service.ApplyUserFilter(query, null).ToList();

            // Assert
            Assert.Empty(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}