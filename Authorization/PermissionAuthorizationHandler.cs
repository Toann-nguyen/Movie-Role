using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MvcMovie.Services;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Authorization
{
    // Handler that checks if user has the required permission
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionService _permissionService;

        public PermissionAuthorizationHandler(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            var user = context.User;
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null && await _permissionService.HasPermissionAsync(userId, requirement.PermissionName))
            {
                context.Succeed(requirement);
            }
        }
    }
}