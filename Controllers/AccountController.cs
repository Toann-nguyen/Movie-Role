using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MvcMovie.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AccountController> _logger;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email!);
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password!, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Movies", new { area = "Admin" });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản với email này.");
                }
            }

            return View(model);
        }


        // Đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password!);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // Đăng xuất
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        // xu ly dang nhap bang Google 

        [HttpPost]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action("GoogleResponse", "Account", new { returnUrl }, Request.Scheme);

            // var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                // Thêm các thông tin cần thiết vào properties
                Items =
            {
                { "returnUrl", returnUrl },
                { "scheme", GoogleDefaults.AuthenticationScheme },
            }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // [HttpGet]
        // public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        // {
        //     var authenticateResult = await HttpContext.AuthenticateAsync();
        //     if (!authenticateResult.Succeeded)
        //         return RedirectToAction(nameof(Login));


        //     // Lấy thông tin người dùng từ Google
        //     var claims = authenticateResult.Principal.Identities.FirstOrDefault()?.Claims;
        //     // Thêm logic lưu vào database nếu cần
        //     return LocalRedirect(returnUrl);
        // }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
                return RedirectToAction(nameof(Login));

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToAction(nameof(Login));

            // Kiểm tra và tạo user nếu chưa tồn tại
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email!);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return RedirectToAction(nameof(Login));
            }

            // Sign in user
            await _signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect(returnUrl);
        }
        // ket thuc xu ly dang nhap Google

        // Tạo tài khoản Admin mặc định
        public static async Task CreateAdminUser(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Kiểm tra xem vai trò Admin có tồn tại chưa, nếu chưa thì tạo
            var roleExist = await roleManager.RoleExistsAsync("Admin");
            if (!roleExist)
            {
                // Tạo vai trò "Admin" nếu chưa tồn tại
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Kiểm tra xem tài khoản Admin đã tồn tại chưa
            var adminUser = await userManager.FindByNameAsync("Admin");
            if (adminUser == null)
            {
                // Tạo tài khoản Admin mới
                var user = new User { UserName = "Admin", Email = "admin@example.com", Status = UserStatus.Active };
                var createResult = await userManager.CreateAsync(user, "Admin@123");

                if (createResult.Succeeded)
                {
                    // Thêm tài khoản Admin vào vai trò "Admin"
                    var addToRoleResult = await userManager.AddToRoleAsync(user, "Admin");

                    if (addToRoleResult.Succeeded)
                    {
                        Console.WriteLine("Admin role assigned successfully.");
                    }
                    else
                    {
                        // Nếu thêm vai trò thất bại, in ra lỗi
                        foreach (var error in addToRoleResult.Errors)
                        {
                            Console.WriteLine($"Error adding user to role: {error.Description}");
                        }
                    }
                }
                else
                {
                    // In ra các lỗi nếu có
                    foreach (var error in createResult.Errors)
                    {
                        Console.WriteLine($"Error creating user: {error.Description}");
                    }
                }
            }
            else
            {
                // Nếu tài khoản Admin đã tồn tại, gán lại vai trò "Admin"
                var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
                if (!isInRole)
                {
                    var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                    if (addToRoleResult.Succeeded)
                    {
                        Console.WriteLine("Admin role assigned successfully.");
                    }
                    else
                    {
                        foreach (var error in addToRoleResult.Errors)
                        {
                            Console.WriteLine($"Error adding user to role: {error.Description}");
                        }
                    }

                }
            }
        }
    }
}
