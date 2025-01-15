using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly MvcMovieContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionService(MvcMovieContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = await _context.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            return await _context.Set<RolePermission>()
                .Include(rp => rp.Permission)
                .AnyAsync(rp =>
                    roleIds.Contains(rp.RoleId!) &&
                    rp.Permission!.Name == permissionName);
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Enumerable.Empty<Permission>();

            var userRoles = await _userManager.GetRolesAsync(user);
            var roleIds = await _context.Roles
                .Where(r => userRoles.Contains(r.Name))
                .Select(r => r.Id)
                .ToListAsync();

            return await _context.Set<RolePermission>()
                .Include(rp => rp.Permission)
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();
        }
        public async Task<bool> GrantPermissionToRoleAsync(string roleId, string permissionId)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            try
            {
                await _context.Set<RolePermission>().AddAsync(rolePermission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokePermissionFromRoleAsync(string roleId, string permissionId)
        {
            var rolePermission = await _context.Set<RolePermission>()
                .FindAsync(roleId, permissionId);

            if (rolePermission == null) return false;

            try
            {
                _context.Set<RolePermission>().Remove(rolePermission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}