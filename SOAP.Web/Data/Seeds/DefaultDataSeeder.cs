using Microsoft.EntityFrameworkCore;
using SOAP.Web.Models.Entities;
using SOAP.Web.Utilities.Constants;

namespace SOAP.Web.Data.Seeds
{
    public static class DefaultDataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Schools
            await SeedSchoolsAsync(context);

            // Seed Sample School Students (for testing)
            await SeedSchoolStudentsAsync(context);

            // Seed Default Admin User
            await SeedDefaultAdminAsync(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedSchoolsAsync(ApplicationDbContext context)
        {
            if (await context.Schools.AnyAsync())
                return;

            var schools = new List<School>
            {
                new School
                {
                    Name = "Alliance High School",
                    Code = "AHS001",
                    County = "Kiambu",
                    ContactPhone = "0722123456",
                    ContactEmail = "admin@alliance.ac.ke",
                    IsActive = true
                },
                new School
                {
                    Name = "Starehe Boys Centre",
                    Code = "SBC002",
                    County = "Nairobi",
                    ContactPhone = "0733234567",
                    ContactEmail = "admin@starehe.ac.ke",
                    IsActive = true
                },
                new School
                {
                    Name = "Kenya High School",
                    Code = "KHS003",
                    County = "Nairobi",
                    ContactPhone = "0744345678",
                    ContactEmail = "admin@kenyahigh.ac.ke",
                    IsActive = true
                }
            };

            await context.Schools.AddRangeAsync(schools);
        }

        private static async Task SeedSchoolStudentsAsync(ApplicationDbContext context)
        {
            if (await context.SchoolStudents.AnyAsync())
                return;

            var allianceSchool = await context.Schools.FirstOrDefaultAsync(s => s.Code == "AHS001");
            if (allianceSchool == null) return;

            var schoolStudents = new List<SchoolStudent>
            {
                new SchoolStudent
                {
                    KcpeIndexNumber = "12345678901",
                    StudentName = "John Doe Mwangi",
                    KcpeScore = 350,
                    SchoolId = allianceSchool.Id,
                    Year = DateTime.Now.Year,
                    HasApplied = false
                },
                new SchoolStudent
                {
                    KcpeIndexNumber = "12345678902",
                    StudentName = "Mary Wanjiku Kamau",
                    KcpeScore = 380,
                    SchoolId = allianceSchool.Id,
                    Year = DateTime.Now.Year,
                    HasApplied = false
                },
                new SchoolStudent
                {
                    KcpeIndexNumber = "12345678903",
                    StudentName = "Peter Kiprotich Koech",
                    KcpeScore = 365,
                    SchoolId = allianceSchool.Id,
                    Year = DateTime.Now.Year,
                    HasApplied = false
                }
            };

            await context.SchoolStudents.AddRangeAsync(schoolStudents);
        }

        private static async Task SeedDefaultAdminAsync(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync())
                return;

            var allianceSchool = await context.Schools.FirstOrDefaultAsync(s => s.Code == "AHS001");
            if (allianceSchool == null) return;

            var defaultAdmin = new User
            {
                PhoneNumber = "0722000000", // Default admin phone
                Role = UserRoles.SchoolAdmin,
                SchoolId = allianceSchool.Id,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await context.Users.AddAsync(defaultAdmin);

            // Create initial consent record for the admin
            var consent = new DataProcessingConsent
            {
                UserId = defaultAdmin.Id.ToString(),
                ConsentType = "SYSTEM_ADMIN",
                Purpose = "System administration and school management",
                ConsentGiven = true,
                ConsentVersion = "1.0",
                ConsentDate = DateTimeOffset.UtcNow,
                IsActive = true
            };

            await context.DataProcessingConsents.AddAsync(consent);
        }
    }
}