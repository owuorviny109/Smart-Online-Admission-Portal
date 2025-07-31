using Microsoft.EntityFrameworkCore;
using SOAP.Web.Data;
using SOAP.Web.Services;
using SOAP.Web.Services.Interfaces;
using SOAP.Web.Configuration;
using SOAP.Web.Authorization;
using SOAP.Web.Middleware;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;

namespace SOAP.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure Entity Framework with SQL Server
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure Data Protection for sensitive data encryption
            builder.Services.AddDataProtection()
                .SetApplicationName(builder.Configuration["Security:DataProtection:ApplicationName"] ?? "SOAP.Web")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(
                    int.Parse(builder.Configuration["Security:DataProtection:KeyLifetime"] ?? "90")));

            // Configure Memory Cache for rate limiting
            builder.Services.AddMemoryCache();

            // Configure Session Management with security
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(
                    int.Parse(builder.Configuration["Security:Authentication:SessionTimeoutMinutes"] ?? "20"));
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "__SOAP_Session";
            });

            // Configure Authentication with security policies
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "SOAP_Cookies";
                options.DefaultChallengeScheme = "SOAP_Cookies";
            })
            .AddCookie("SOAP_Cookies", options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "__SOAP_Auth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(
                    int.Parse(builder.Configuration["Security:Authentication:SessionTimeoutMinutes"] ?? "20"));
                options.SlidingExpiration = true;
            });

            // Configure Authorization with custom policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SchoolAdminOnly", policy =>
                    policy.RequireRole("SchoolAdmin", "SuperAdmin"));
                
                options.AddPolicy("ParentOnly", policy =>
                    policy.RequireRole("Parent"));
                
                options.AddPolicy("SameSchoolAccess", policy =>
                    policy.Requirements.Add(new SameSchoolRequirement()));
                
                options.AddPolicy("ParentOwnDataOnly", policy =>
                    policy.Requirements.Add(new ParentDataAccessRequirement()));
            });

            // Register Authorization Handlers
            builder.Services.AddScoped<IAuthorizationHandler, SameSchoolRequirementHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, ParentDataAccessRequirementHandler>();

            // Configure Anti-Forgery with enhanced security
            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "__SOAP_CSRF";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // Register Application Services with Dependency Injection (Following DIP)
            builder.Services.AddScoped<IApplicationService, ApplicationService>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<ISmsService, SmsService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();
            builder.Services.AddScoped<IDataProtectionService, DataProtectionService>();
            builder.Services.AddScoped<IRateLimitingService, RateLimitingService>();

            // Register Notification Services (Strategy Pattern - OCP)
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationStrategy, Services.Strategies.SmsNotificationStrategy>();
            builder.Services.AddScoped<INotificationStrategy, Services.Strategies.EmailNotificationStrategy>();

            // Register Document Validation Services (Strategy Pattern - OCP)
            builder.Services.AddScoped<IDocumentValidator, Services.Validators.PdfDocumentValidator>();
            builder.Services.AddScoped<IDocumentValidator, Services.Validators.ImageDocumentValidator>();

            // Configure Security Services
            builder.Services.Configure<SmsConfig>(
                builder.Configuration.GetSection("Sms"));
            builder.Services.Configure<SecurityConfig>(
                builder.Configuration.GetSection("Security"));

            // Configure Background Services
            builder.Services.AddHostedService<Services.BackgroundServices.SecurityMonitoringService>();
            builder.Services.AddHostedService<Services.BackgroundServices.DataRetentionService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Add Security Headers Middleware
            app.UseSecurityHeaders();

            app.UseRouting();

            // Add Session before Authentication
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
