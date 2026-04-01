using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CloudinaryDotNet;
using SafetyMap.Core.Contracts;
using SafetyMap.Core.DTOs;
using SafetyMap.Core.Services;
using SafetyMapData;
using SafetyMapData.Entities;
using SafetyMapData.Configurations.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<SafetyMapDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Identity with Roles
builder.Services.AddDefaultIdentity<UserIdentity>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 5;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<SafetyMapDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
if (!string.IsNullOrEmpty(googleClientId))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    });
}

var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"];
if (!string.IsNullOrEmpty(facebookAppId))
{
    authBuilder.AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? string.Empty;
        options.Scope.Clear();
    });
}

// Cloudinary
var cloudinarySettings = builder.Configuration.GetSection("CloudinarySettings").Get<CloudinarySettings>();

var cloudName = !string.IsNullOrEmpty(cloudinarySettings?.CloudName) ? cloudinarySettings.CloudName : "dummy_cloud_name";
var apiKey = !string.IsNullOrEmpty(cloudinarySettings?.ApiKey) ? cloudinarySettings.ApiKey : "dummy_api_key";
var apiSecret = !string.IsNullOrEmpty(cloudinarySettings?.ApiSecret) ? cloudinarySettings.ApiSecret : "dummy_api_secret";

var account = new Account(cloudName, apiKey, apiSecret);
var cloudinary = new Cloudinary(account);
builder.Services.AddSingleton(cloudinary);

builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICrimeCategoryService, CrimeCategoryService>();
builder.Services.AddScoped<INeighborhoodService, NeighborhoodService>();
builder.Services.AddScoped<ICrimeStatisticService, CrimeStatisticService>();
builder.Services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
builder.Services.AddScoped<IMapService, MapService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserCrimeReportService, UserCrimeReportService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailQueueService, EmailQueueService>();
builder.Services.AddHostedService<EmailBackgroundService>();

builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SafetyMapDbContext>();
        await context.Database.MigrateAsync();
        
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("An error occurred while seeding the database: " + ex.Message);
    }
}

// Health check endpoint for Docker
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();
