using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using AutoMapper;
using MvcMovie.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using MvcMovie.Models;
using MvcMovie.Controllers;
using MvcMovie.Authorization;
using MvcMovie.Utils.ConfigOptions.VNPay;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<MvcMovieContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("MvcMovieContext") ?? throw new InvalidOperationException("Connection string 'MvcMovieContext' not found.")));



        // Đăng ký Identity vào container DI
        // builder.Services.AddIdentity<User, IdentityRole>()
        //     .AddEntityFrameworkStores<MvcMovieContext>()
        //     .AddDefaultTokenProviders();

        //File Storage

        builder.Services.AddTransient<IStorageService, FileStorageService>();

        // note
        builder.Services.AddRazorPages();


        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;

                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<MvcMovieContext>()
                .AddDefaultTokenProviders();
        //end note
        // Thêm Authentication


        // Add authorization with permissions
        builder.Services.AddAuthorization(options =>
        {
            // Get all permissions from database
            using var scope = builder.Services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MvcMovieContext>();

            var permissions = context.Set<Permission>().ToList();
            string[] roles = { "Member", "Manager" };
            foreach (var role in roles)
            {

            }
            // Register a policy for each permission
            foreach (var permission in permissions)
            {
                options.AddPolicy($"Permission_{permission.Name}",
                    policy => policy.Requirements.Add(new PermissionRequirement(permission.Name!)));
            }
        });



        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddScoped<IMovieService, MovieService>();
        //VNpay 
        builder.Services.AddTransient<IVNPayService, VNPayService>();
        builder.Services.Configure<VNPayConfigOptions>(builder.Configuration.GetSection("VnPay"));

        builder.Services.AddScoped<IPermissionService, PermissionService>();
        // Register the permission handler
        builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<MvcMovieContext>();

            string[] roles = { "Member", "Manager" };


            context.Database.EnsureCreated();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            await Initialize(services, userManager, roleManager);
        }




        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapStaticAssets();

        app.MapControllers();

        app.MapRazorPages();


        app.MapControllerRoute(
                    name: "account",
                    pattern: "account/{action=Login}/{id?}",
                    defaults: new { controller = "Account" });


        app.MapControllerRoute(
            name: "Admin",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();



        app.Run();
    }

    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync("Admin");
        if (role == null)
        {
            role = new IdentityRole("Admin");
            await roleManager.CreateAsync(role);
        }

        var user = await userManager.FindByEmailAsync("admin@admin.com");
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com"
            };
            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Admin");
        }
        // Tạo tài khoản khác (hello@gmail.com)
        var anotherUser = await userManager.FindByEmailAsync("toan@gmail.com");
        if (anotherUser == null)
        {
            anotherUser = new ApplicationUser
            {
                UserName = "toan@gmail.com",
                Email = "toan@gmail.com",
                EmailConfirmed = true // Xác thực email
            };
            var result = await userManager.CreateAsync(anotherUser, "Password123!"); // Mật khẩu mặc định
            if (result.Succeeded)
            {
                // Thêm vai trò nếu cần
                await userManager.AddToRoleAsync(anotherUser, "Admin");
            }
        }
        else
        {
            // Nếu tài khoản đã tồn tại, chỉ cần thêm vai trò (nếu chưa có)
            if (!await userManager.IsInRoleAsync(anotherUser, "Admin"))
            {
                await userManager.AddToRoleAsync(anotherUser, "Admin");
            }
        }

    }

    // khoi tao gia tri ban dau trong database
    public static async Task SeedRolesAndPermissions(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<MvcMovieContext>();

        // Create roles
        string[] roleNames = { "Admin", "User", "Editor" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create permissions
        var permissions = new[]
        {
        new Permission { Id = "1", Name = "ViewMovies", Description = "Can view movies", Group = "Movies" },
        new Permission { Id = "2", Name = "CreateMovies", Description = "Can create movies", Group = "Movies" },
        new Permission { Id = "3", Name = "EditMovies", Description = "Can edit movies", Group = "Movies" },
        new Permission { Id = "4", Name = "DeleteMovies", Description = "Can delete movies", Group = "Movies" }
    };

        foreach (var permission in permissions)
        {
            if (!await context.Set<Permission>().AnyAsync(p => p.Id == permission.Id))
            {
                context.Set<Permission>().Add(permission);
            }
        }

        await context.SaveChangesAsync();
    }

}