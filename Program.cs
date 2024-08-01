using ConnectingDatabase.Data;
using ConnectingDatabase.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DatabaseConnection")));

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.IOTimeout = TimeSpan.FromSeconds(1000);
    options.Cookie.Name = ".MySampleMVCWeb.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Path = "/";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register Custom Logger Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddTransient<CustomLoggerService>();

var app = builder.Build();

// Ensure the CustomLogs directory exists
string logDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "CustomLogs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Define endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");
});

app.Run();