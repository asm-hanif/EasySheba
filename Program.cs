using EasySheba.Data;
using EasySheba.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Identity + Roles Setup
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// MVC Setup
builder.Services.AddControllersWithViews();
// Razor Pages (required for existing Razor Pages routes)
builder.Services.AddRazorPages();

// Email Service Setup
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();


// Appointment Limit Service
builder.Services.AddScoped<IAppointmentLimitService, AppointmentLimitService>();

// Appointment Reminder Background Service
builder.Services.AddHostedService<AppointmentReminderService>();

var app = builder.Build();

// Auto Create Roles + Default Super Admin on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Create Roles
    string[] roles = { "Patient", "HospitalAdmin", "SuperAdmin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create Default Super Admin
    string superAdminEmail = "admin@easysheba.com";
    string superAdminPassword = "Admin@123";

    var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

    if (superAdminUser == null)
    {
        var user = new IdentityUser
        {
            UserName = superAdminEmail,
            Email = superAdminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, superAdminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "SuperAdmin");
        }
    }
}

// Middleware Pipeline
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

// Ensure attribute-routed controllers are mapped (required for [Route]/[HttpGet] on controllers)
app.MapControllers();
// Ensure Razor Pages are mapped
app.MapRazorPages();

// Default Route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();