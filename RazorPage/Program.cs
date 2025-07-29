using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using RazorPage.Helpers;
using RazorPage.Middleware;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services;
using Services.Implementations;
using Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RazorPage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Entity Framework
            builder.Services.AddDbContext<BloodDonationSupportContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            // Register all repositories and services
            builder.Services.AddScoped<IHealthCheckRepository, HealthCheckRepository>();
            builder.Services.AddScoped<IHealthCheckService, HealthCheckService>();

            builder.Services.AddScoped<IBloodDonationRepository, BloodDonationRepository>();
            builder.Services.AddScoped<IBloodDonationService, BloodDonationService>();

            builder.Services.AddScoped<IDonorRepository, DonorRepository>();
            builder.Services.AddScoped<IDonorService, DonorService>();

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            builder.Services.AddScoped<IDonationHistoryRepository, DonationHistoryRepository>();
            builder.Services.AddScoped<IDonationHistoryService, DonationHistoryService>();

            builder.Services.AddScoped<IBlogRepository, BlogRepository>();
            builder.Services.AddScoped<IBlogService, BlogService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IBloodRequestService, BloodRequestService>();
            builder.Services.AddScoped<IBloodRequestRepository, BloodRequestRepository>();

            builder.Services.AddScoped<IBloodRecipientRepository, BloodRecipientRepository>();
            builder.Services.AddScoped<IBloodTypeRepository, BloodTypeRepository>();
            builder.Services.AddScoped<IBloodComponentRepository, BloodComponentRepository>();
            builder.Services.AddScoped<IBloodManagementService, BloodManagementService>();
            builder.Services.AddScoped<IBloodUnitRepository, BloodUnitRepository>();
            builder.Services.AddScoped<IBloodUnitService, BloodUnitService>();

            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IJwtService, JwtService>();

            // Configure Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
            })
            // Google authentication is commented out until proper configuration is provided
            // .AddGoogle(options =>
            // {
            //     options.ClientId = googleSection["ClientId"];
            //     options.ClientSecret = googleSection["ClientSecret"];
            //     options.Scope.Add("email");
            //     options.Scope.Add("profile");
            // })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                        throw new InvalidOperationException("JWT Key is not configured")))
                };
            });

            // Add Authorization Policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireStaffRole", policy => policy.RequireRole("Staff"));
                options.AddPolicy("RequireAdminOrStaffRole", policy => policy.RequireRole("Admin", "Staff"));
                options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
                options.AddPolicy("RequireMemberRole", policy => policy.RequireRole("Member", "Customer", "Admin", "Staff"));
            });

            // Add services to the container.
            builder.Services.AddRazorPages(options =>
            {
                // Configure authorization for specific page folders
                options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
                options.Conventions.AuthorizeFolder("/Staff", "RequireAdminOrStaffRole");
                options.Conventions.AuthorizeFolder("/Member", "RequireMemberRole");
                // Allow anonymous access to login and register pages
                options.Conventions.AllowAnonymousToPage("/Account/Login");
                options.Conventions.AllowAnonymousToPage("/Account/Register");
                // Allow anonymous access to BlogPosts page
                options.Conventions.AllowAnonymousToPage("/Member/BlogPosts");
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
