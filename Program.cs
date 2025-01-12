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
using MvcMovie.Models;
using MvcMovie.Controllers;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<MvcMovieContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("MvcMovieContext") ?? throw new InvalidOperationException("Connection string 'MvcMovieContext' not found.")));

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddScoped<IMovieService, MovieService>();


        // Đăng ký Identity vào container DI
        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<MvcMovieContext>()
            .AddDefaultTokenProviders();

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
        builder.Services.AddAuthentication(options =>
        {
            // options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // Thiết lập scheme mặc định cho cookie authentication
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // Thiết lập scheme mặc định cho challenge (khi người dùng chưa đăng nhập)
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // Thiết lập scheme mặc định cho đăng nhập từ bên ngoài
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
            {
                // Cấu hình cookie authentication
                options.LoginPath = "/Account/Login";  // Đường dẫn trang đăng nhập
                options.LogoutPath = "/Account/Logout"; // Đường dẫn đăng xuất
                options.AccessDeniedPath = "/Account/AccessDenied"; // Trang hiển thị khi không có quyền truy cập

                // Cấu hình thời gian sống của cookie
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true; // Cookie sẽ được gia hạn mỗi khi người dùng truy cập

                // Tăng cường bảo mật cho cookie
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            // options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            // options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            // Cấu hình Google authentication
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

            // Tùy chỉnh scope để lấy thêm thông tin từ Google
            options.Scope.Add("email");
            options.Scope.Add("profile");

            // Tùy chỉnh claims mapping
            options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
            options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");

            // Xử lý events
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    // Bạn có thể thêm xử lý tùy chỉnh ở đây khi ticket được tạo
                },
                OnRedirectToAuthorizationEndpoint = context =>
                {
                    // Tùy chỉnh URL chuyển hướng nếu cần
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                }
            };

            // Thiết lập callback path
            options.CallbackPath = "/signin-google"; // Đường dẫn callback mặc định

        });



        var app = builder.Build();

        // Tạo tài khoản Admin mặc định
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await AccountController.CreateAdminUser(userManager, roleManager); // Tạo tài khoản Admin mặc định
        }

        // Tạo tài khoản Admin mặc định sau khi cấu hình các dịch vụ
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await AccountController.CreateAdminUser(userManager, roleManager); // Tạo tài khoản Admin mặc định
        }



        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

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
    }

}