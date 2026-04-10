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
    // Convert postgres://user:pass@host:port/db to Npgsql format
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};SSL Mode=Require;Trust Server Certificate=True";
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

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
    context.Database.Migrate();
    
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