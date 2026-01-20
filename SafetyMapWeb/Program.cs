using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyMapData;
using SafetyMapData.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<SafetyMapDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<UserIdentity>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; 
    options.Password.RequiredLength = 5;
})
.AddEntityFrameworkStores<SafetyMapDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages(); // Not needed if using MVC Views for auth


var app = builder.Build();

// Configure the HTTP request pipeline.
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();