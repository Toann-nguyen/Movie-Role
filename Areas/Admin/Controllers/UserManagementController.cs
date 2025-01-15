// Controllers/UserManagementController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Authorization;
using MvcMovie.Models;
using MvcMovie.Services;
using MvcMovie.ViewModels;



namespace MvcMovie.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPermissionService _permissionService;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IPermissionService permissionService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

                userViewModels.Add(new UserViewModel
                {
                    User = user,
                    Roles = roles,
                    Permissions = permissions
                });
            }

            return View(userViewModels);
        }
        //Edit User Roles for GET

        public async Task<IActionResult> EditUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditUserRolesViewModel
            {
                UserId = userId,
                UserName = user.UserName!,
                Roles = allRoles.Select(r => new RoleViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name!)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserRoles(EditUserRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.Roles.Where(x => x.IsSelected && !userRoles.Contains(x.RoleName!)).Select(x => x.RoleName!);
            var rolesToRemove = model.Roles.Where(x => !x.IsSelected && userRoles.Contains(x.RoleName!)).Select(x => x.RoleName!);

            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("CreateMovies")]
        public IActionResult Create()
        {
            return View();
        }
    }
}