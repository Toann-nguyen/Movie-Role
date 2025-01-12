using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MvcMovie.Models;
public class User : IdentityUser
{
    // Trạng thái hoạt động của người dùng
    public UserStatus Status { get; set; } = UserStatus.Active; // Mặc định là Active

    // Thông tin cá nhân bổ sung
    public DateTime? Birth { get; set; }
    public string? Address { get; set; }
    public string? ImageURL { get; set; }
}


// Enum trạng thái người dùng
public enum UserStatus
{
    Active,
    Inactive
}

public class ManageRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
}
