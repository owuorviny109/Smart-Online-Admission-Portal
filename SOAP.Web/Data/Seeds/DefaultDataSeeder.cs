using Microsoft.EntityFrameworkCore;
using SOAP.Web.Models.Entities;

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
            
            // Seed Users
            await SeedUsersAsync(context);
            
            // Seed Sample Students (for testing)
            await SeedSchoolStudentsAsync(context);
            
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
                    ContactEmail = "info@alliance.ac.ke",
                    IsActive = true
                },
                new School
                {
                    Name = "Starehe Boys Centre",
                    Code = "SBC002",
                    County = "Nairobi",
                    ContactPhone = "0733234567",
                    ContactEmail = "info@starehe.ac.ke",
                    IsActive = true
                },
                new School
                {
                    Name = "Kenya High School",
                    Code = "KHS003",
                    County = "Nairobi",
                    ContactPhone = "0744345678",
                    ContactEmail = "info@kenyahigh.ac.ke",
                    IsActive = true
                },
                new School
                {
                    Name = "Mang'u High School",
                    Code = "MHS004",
                    County = "Kiambu",
                    ContactPhone = "0755456789",
                    ContactEmail = "info@mangu.ac.ke",
                    IsActive = true
                },
                new School
                {
                    Name = "Loreto High School Limuru",
                    Code = "LHL005",
                    County = "Kiambu",
                    ContactPhone = "0766567890",
                    ContactEmail = "info@loretolimuru.ac.ke",
                    IsActive = true
                }
            };
            
            await context.Schools.AddRangeAsync(schools);
        }
        
        private static async Task SeedUsersAsync(ApplicationDbContext context)
        {
            if (await context.Users.AnyAsync())
                return;
            
            var schools = await context.Schools.ToListAsync();
            var users = new List<User>();
            
            // Create admin users for each school
            foreach (var school in schools)
            {
                users.Add(new User
                {
                    PhoneNumber = $"07{Random.Shared.Next(10000000, 99999999)}",
                    Role = "Admin",
                    SchoolId = school.Id,
                    IsActive = true
                });
            }
            
            // Create a super admin
            users.Add(new User
            {
                PhoneNumber = "0700000000",
                Role = "SuperAdmin",
                SchoolId = null,
                IsActive = true
            });
            
            await context.Users.AddRangeAsync(users);
        }
        
        private static async Task SeedSchoolStudentsAsync(ApplicationDbContext context)
        {
            if (await context.SchoolStudents.AnyAsync())
                return;
            
            var schools = await context.Schools.ToListAsync();
            var students = new List<SchoolStudent>();
            var currentYear = DateTime.Now.Year;
            
            foreach (var school in schools)
            {
                // Generate 50 sample students per school
                for (int i = 1; i <= 50; i++)
                {
                    var kcpeNumber = $"{currentYear}{school.Code.Substring(0, 3)}{i:D5}";
                    
                    students.Add(new SchoolStudent
                    {
                        KcpeIndexNumber = kcpeNumber,
                        StudentName = GenerateRandomName(),
                        KcpeScore = Random.Shared.Next(250, 500),
                        SchoolId = school.Id,
                        Year = currentYear,
                        HasApplied = false
                    });
                }
            }
            
            await context.SchoolStudents.AddRangeAsync(students);
        }
        
        private static string GenerateRandomName()
        {
            var firstNames = new[]
            {
                "John", "Mary", "Peter", "Grace", "David", "Faith", "James", "Joyce",
                "Michael", "Catherine", "Daniel", "Margaret", "Joseph", "Elizabeth",
                "Samuel", "Rose", "Benjamin", "Sarah", "Emmanuel", "Ruth"
            };
            
            var lastNames = new[]
            {
                "Mwangi", "Wanjiku", "Kamau", "Njeri", "Kiprotich", "Chebet",
                "Ochieng", "Akinyi", "Maina", "Wambui", "Kiplagat", "Jepkoech",
                "Otieno", "Awino", "Mutua", "Muthoni", "Kiptoo", "Chepkemoi"
            };
            
            var firstName = firstNames[Random.Shared.Next(firstNames.Length)];
            var lastName = lastNames[Random.Shared.Next(lastNames.Length)];
            
            return $"{firstName} {lastName}";
        }
    }
}