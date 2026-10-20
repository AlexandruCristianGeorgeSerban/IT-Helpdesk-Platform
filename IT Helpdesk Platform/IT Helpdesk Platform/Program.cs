using IT_Helpdesk_Platform.Models;
using IT_Helpdesk_Platform.Repositories;
using IT_Helpdesk_Platform.Repositories.Interfaces;
using IT_Helpdesk_Platform.Services;
using IT_Helpdesk_Platform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Register HelpdeskContext with the SQL Server provider
builder.Services.AddDbContext<HelpdeskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 1. AUTHENTICATION SYSTEM SETUP ---
// This must be the ONLY Identity registration
builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Set to false so users can log in immediately without needing an email confirmation link
    options.SignIn.RequireConfirmedAccount = false;

    // LOCKOUT
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<HelpdeskContext>()
.AddDefaultTokenProviders();

// --- 2. CONFIGURE APPLICATION COOKIE ---
// Defines where users are redirected if they are not logged in or lack permissions
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Home/GuestDashboard";
});

// --- 3. DEPENDENCY INJECTION: REPOSITORIES ---
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

// --- 4. DEPENDENCY INJECTION: SERVICES ---
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// --- 5. SECURITY PIPELINE ---
// IMPORTANT: UseAuthentication MUST be called before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=GuestDashboard}/{id?}");

// --- 6. ROLE CREATION ON STARTUP ---
// Ensures that the default roles exist in the database when the app starts
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    string[] roleNames = { "Admin", "Agent", "User" };

    foreach (var roleName in roleNames)
    {
        // If the role doesn't exist, create it
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }
}

// --- 7. SEEDING DATABASE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HelpdeskContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        context.Database.Migrate();

        await IT_Helpdesk_Platform.Data.DbSeeder.SeedAsync(services, context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// -----------------------------

app.Run();