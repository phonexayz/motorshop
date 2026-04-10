using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Services;

// Fix Npgsql DateTime translation errors (NotSupportedException for timestamp with time zone vs without time zone)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Support DATABASE_URL from Railway/Render if present
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    try 
    {
        // Replace postgresql:// with postgres:// for Uri class if needed
        var formattedUrl = databaseUrl.Replace("postgresql://", "postgres://");
        var uri = new Uri(formattedUrl);
        
        var userInfo = uri.UserInfo.Split(':');
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        
        connectionString = $"Host={host};Port={port};Username={user};Password={password};Database={database};SSL Mode=Require;Trust Server Certificate=True;Pooling=true;";
        
        Console.WriteLine($"[Cloud] Attempting to connect to host: {host}, database: {database}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Cloud] Error parsing DATABASE_URL: {ex.Message}");
        if (!databaseUrl.StartsWith("postgres"))
        {
            connectionString = databaseUrl;
        }
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Support dynamic PORT assigned by Railway
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnv))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portEnv}");
}

// Register services
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IRepairOrderService, RepairOrderService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IAuditService, AuditService>();

var app = builder.Build();

// Migrate database and seed admin user
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try 
    {
        Console.WriteLine("[Cloud] Running database migrations...");
        context.Database.Migrate();
        Console.WriteLine("[Cloud] Database migration successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Cloud] Migration failed: {ex.Message}");
    }
    
    if (!context.Users.Any())
    {
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        authService.RegisterAsync("admin", "password", "Admin").GetAwaiter().GetResult();
    }
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

// Use session
app.UseSession();

app.UseAuthorization();

// Add authentication middleware
app.Use(async (context, next) =>
{
    var authService = context.RequestServices.GetRequiredService<IAuthService>();
    
    // Redirect to login if not authenticated and not on login page
    if (!authService.IsAuthenticated() && 
        !context.Request.Path.StartsWithSegments("/Account/Login"))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();