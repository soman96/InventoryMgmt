using System.Globalization;
using InventoryMgmt.Data;
using InventoryMgmt.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Serilog;
using Serilog.Events;


var builder = WebApplication.CreateBuilder(args);

// Add sessions
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession();

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add the context to the service collection with a connection string
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();
// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});


// Configure logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


// Inject our SendGrid email sender
builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

app.UseStaticFiles();
app.MapStaticAssets();

var scope = app.Services.CreateScope();
await IdentitySeeder.SeedRolesAsync(scope.ServiceProvider);
await IdentitySeeder.SeedAdminAsync(scope.ServiceProvider);

// Configure global errors
if (!app.Environment.IsDevelopment()) 
{
    // Global exception handler for unhandled exceptions (500)
    app.UseExceptionHandler("/Error/ServerError");

    // Handle 404 and other status codes
    app.UseStatusCodePagesWithReExecute("/Error/Status", "?code={0}");

    app.UseHsts(); 
}
else
{
    app.UseDeveloperExceptionPage(); // For development debugging
}


app.UseHttpsRedirection();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

var supportedCultures = new[] { new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});


app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
).WithStaticAssets();

app.Run();