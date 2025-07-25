using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using VitaFlow.Infrastructure.Data;
using VitaFlow.Infrastructure.Repositories.Implements;
using VitaFlow.Infrastructure.Repositories.Interfaces;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Services.Services;
using FluentValidation;
using VitaFlow.Services.Validators;
using VitaFlow.Core.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register UnitOfWork for dependency injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<ApplicationDbContext>>();
builder.Services.AddScoped(typeof(VitaFlow.Infrastructure.Repositories.Interfaces.IGenericRepository<>), typeof(VitaFlow.Infrastructure.Repositories.Implements.GenericRepository<>));

// Register Business Services
builder.Services.AddScoped<IDonationProcessService, DonationProcessService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBloodInventoryService, BloodInventoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IBloodCompatibilityService, BloodCompatibilityService>();
builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<IUserService, UserService>();

// Register Validators
builder.Services.AddScoped<IValidator<Donor>, DonorValidator>();

// Add Memory Cache for performance
builder.Services.AddMemoryCache();

// Add Logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
