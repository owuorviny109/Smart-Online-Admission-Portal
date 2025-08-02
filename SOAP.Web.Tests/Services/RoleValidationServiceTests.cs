using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SOAP.Web.Data;
using SOAP.Web.Models.Entities;
using SOAP.Web.Services;
using SOAP.Web.Utilities.Constants;
using Xunit;

namespace SOAP.Web.Tests.Services
{
    public class RoleValidationServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<RoleValidationService>> _mockLogger;
        private readonly RoleValidationService _service;

        public RoleValidationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<RoleValidationService>>();
            _service = new RoleValidationService(_context, _mockLogger.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var school = new School
            {
                Id = 1,
                Name = "Test High School",
                Code = "THS001",
                County = "Test County",
                IsActive = true
            };

            _context.Schools.Add(school);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CanUserHaveRoleAsync_PlatformAdmin_OnlyAllowsSystemOwner()
        {
            // Arrange
            var systemOwnerPhone = UserRoles.PLATFORM_ADMIN_PHONE;
            var hackerPhone = "+254700000001";

            // Act & Assert
            var systemOwnerResult = await _service.CanUserHaveRoleAsync(systemOwnerPhone, UserRoles.PlatformAdmin);
            var hackerResult = await _service.CanUserHaveRoleAsync(hackerPhone, UserRoles.PlatformAdmin);

            // Assert
            Assert.True(systemOwnerResult);
            Assert.False(hackerResult);
        }

        [Fact]
        public async Task CanUserHaveRoleAsync_SchoolAdmin_RequiresValidSchool()
        {
            // Arrange
            var phoneNumber = "+254700000002";

            // Act & Assert
            var validSchoolResult = await _service.CanUserHaveRoleAsync(phoneNumber, UserRoles.SchoolAdmin, 1);
            var invalidSchoolResult = await _service.CanUserHaveRoleAsync(phoneNumber, UserRoles.SchoolAdmin, 999);
            var noSchoolResult = await _service.CanUserHaveRoleAsync(phoneNumber, UserRoles.SchoolAdmin, null);

            // Assert
            Assert.True(validSchoolResult);
            Assert.False(invalidSchoolResult);
            Assert.False(noSchoolResult);
        }

        [Fact]
        public async Task CanAccessSchoolDataAsync_SchoolAdmin_OnlyAccessesOwnSchool()
        {
            // Arrange
            var schoolAdmin = new User
            {
                Id = 1,
                PhoneNumber = "+254700000003",
                Role = UserRoles.SchoolAdmin,
                SchoolId = 1
            };

            // Act
            var ownSchoolAccess = await _service.CanAccessSchoolDataAsync(schoolAdmin, 1);
            var otherSchoolAccess = await _service.CanAccessSchoolDataAsync(schoolAdmin, 2);

            // Assert
            Assert.True(ownSchoolAccess);
            Assert.False(otherSchoolAccess);
        }

        [Fact]
        public async Task CanAccessSchoolDataAsync_Parent_CannotAccessSchoolData()
        {
            // Arrange
            var parent = new User
            {
                Id = 2,
                PhoneNumber = "+254700000004",
                Role = UserRoles.Parent
            };

            // Act
            var result = await _service.CanAccessSchoolDataAsync(parent, 1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsPrivilegeEscalationAttemptAsync_DetectsUnauthorizedPlatformAdminAttempt()
        {
            // Arrange
            var hacker = new User
            {
                Id = 3,
                PhoneNumber = "+254700000005", // Not the system owner's phone
                Role = UserRoles.Parent
            };

            // Act
            var result = await _service.IsPrivilegeEscalationAttemptAsync(hacker, UserRoles.PlatformAdmin);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsPrivilegeEscalationAttemptAsync_DetectsParentToAdminEscalation()
        {
            // Arrange
            var parent = new User
            {
                Id = 4,
                PhoneNumber = "+254700000006",
                Role = UserRoles.Parent
            };

            // Act
            var result = await _service.IsPrivilegeEscalationAttemptAsync(parent, UserRoles.SchoolAdmin);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetDashboardRoute_ReturnsCorrectRoutes()
        {
            // Act & Assert
            Assert.Equal("/Dashboard", _service.GetDashboardRoute(UserRoles.PlatformAdmin));
            Assert.Equal("/Admin/Dashboard", _service.GetDashboardRoute(UserRoles.SchoolAdmin));
            Assert.Equal("/Parent/Home", _service.GetDashboardRoute(UserRoles.Parent));
            Assert.Equal("/Account/Login", _service.GetDashboardRoute("InvalidRole"));
        }

        [Theory]
        [InlineData(UserRoles.PlatformAdmin, true)]
        [InlineData(UserRoles.SchoolAdmin, true)]
        [InlineData(UserRoles.Parent, true)]
        [InlineData("InvalidRole", false)]
        public void UserRoles_IsValidRole_ValidatesCorrectly(string role, bool expected)
        {
            // Act
            var result = UserRoles.IsValidRole(role);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void UserRoles_CanBePlatformAdmin_OnlyAllowsSystemOwner()
        {
            // Arrange
            var systemOwnerPhone = UserRoles.PLATFORM_ADMIN_PHONE;
            var otherPhone = "+254700000007";

            // Act & Assert
            Assert.True(UserRoles.CanBePlatformAdmin(systemOwnerPhone));
            Assert.False(UserRoles.CanBePlatformAdmin(otherPhone));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}