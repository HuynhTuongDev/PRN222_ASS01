using BusinessObjects;
using BusinessObjects.Models;
using DataAccessObjects;
using DataAccessObjects.Repositories;
using DataAccessObjects.Repositories.Interfaces;
using DataAccessObjects.Services;
using DataAccessObjects.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Add Controllers with Views
builder.Services.AddControllersWithViews();

// 🔹 Authorization policy AdminOnly dựa trên AccountRole = 3
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "AccountRole" && c.Value == "3")));
});

// 🔹 Authentication cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

// 🔹 Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("FUNewsManagementDB");
builder.Services.AddDbContext<FunewsManagementContext>(options =>
    options.UseSqlServer(connectionString));

// 🔹 Register Repositories
builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// 🔹 Register Services
builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// 🔹 Register DbInitializer
builder.Services.AddScoped<DbInitializer>();

// 🔹 Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 🔹 Initialize database and create default admin if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<FunewsManagementContext>();
        var initializer = services.GetRequiredService<DbInitializer>();
        await initializer.InitializeAsync();

        // Lấy cấu hình AdminAccount từ appsettings.json
        var adminConfig = builder.Configuration.GetSection("AdminAccount");
        var adminEmail = adminConfig["Email"];
        var adminPassword = adminConfig["Password"];
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Checking for admin account with email: {Email}", adminEmail);

        if (!await context.SystemAccounts.AnyAsync(x => x.AccountEmail == adminEmail))
        {
            logger.LogInformation("Admin account not found, creating new admin account");

            var maxId = await context.SystemAccounts.MaxAsync(x => (int?)x.AccountId) ?? 0;

            var adminAccount = new SystemAccount
            {
                AccountId = (short)(maxId + 1),
                AccountEmail = adminEmail,
                AccountPassword = adminPassword, 
                AccountRole = 3, // Role 3 = admin
                AccountName = "System Administrator"
            };

            await context.SystemAccounts.AddAsync(adminAccount);
            await context.SaveChangesAsync();
            logger.LogInformation("Admin account created successfully with ID: {Id}", adminAccount.AccountId);
        }
        else
        {
            logger.LogInformation("Admin account already exists");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

// 🔹 Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// 🔹 Map default controller route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
