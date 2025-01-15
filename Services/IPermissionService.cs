using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;


namespace MvcMovie.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string userId, string permissionName);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
        Task<bool> GrantPermissionToRoleAsync(string roleId, string permissionId);
        Task<bool> RevokePermissionFromRoleAsync(string roleId, string permissionId);
    }
}