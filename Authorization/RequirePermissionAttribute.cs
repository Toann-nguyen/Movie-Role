using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Authorization
{
    // Custom attribute for requiring specific permissions
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permissionName)
        {
            Policy = $"Permission_{permissionName}";
        }
    }
}