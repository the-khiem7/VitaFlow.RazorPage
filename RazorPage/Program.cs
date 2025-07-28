using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Models;
using Repositories.Implementations;
using Repositories.Interfaces;
using Services;
using Services.Implementations;
using Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using RazorPage.Helpers;
using RazorPage.Middleware;

namespace RazorPage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var googleSection = builder.Configuration.GetSection("Authentication:Google");

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
            .AddGoogle(options =>
            {
                options.ClientId = googleSection["ClientId"];
                options.ClientSecret = googleSection["ClientSecret"];
                options.Scope.Add("email");
                options.Scope.Add("profile");
            })
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
                options.Conventions.AuthorizeFolder("/Staff", "RequireStaffRole");
                options.Conventions.AuthorizeFolder("/Member", "RequireMemberRole");
                // Allow anonymous access to login and register pages
                options.Conventions.AllowAnonymousToPage("/Account/Login");
                options.Conventions.AllowAnonymousToPage("/Account/Register");
            });

            // Add controllers for API endpoints (hybrid approach)
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // Configure Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blood Donation API", Version = "v1" });
                c.UseInlineDefinitionsForEnums();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
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
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blood Donation API V1");
                });
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers(); // Support for API controllers

            app.Run();
        }
    }
}
