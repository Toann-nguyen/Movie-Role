using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MvcMovie.Models;
public class User : IdentityUser
{
    // Trạng thái hoạt động của người dùng
    public UserStatus Status { get; set; } = UserStatus.Active; // Mặc định là Active

    // Thông tin cá nhân bổ sung
    public string UserId { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }

    public string? ImageURL { get; set; }
}


// Enum trạng thái người dùng
public enum UserStatus
{
    Active,
    Inactive,
    Locked
}

public class Role : IdentityRole
{
    public ICollection<Permission>? Permissions { get; set; }

    public Role() : base()
    {
        Permissions = new HashSet<Permission>();
    }

}

// 2. Cập nhật Role entity để thêm relationship với Permission
public class Permission
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }        // Tên quyền (ví dụ: "CreateMovie")
    public string? Description { get; set; }  // Mô tả quyền
    public string? Group { get; set; }        // Nhóm quyền (ví dụ: "Movies", "Users")

    // Relationship với Role (many-to-many)
    public ICollection<Role>? Roles { get; set; }
    
    public Permission()
    {
        Roles = new HashSet<Role>();
    }
}

// 3. Tạo bảng trung gian để map quan hệ many-to-many
public class RolePermission
{
    public string? RoleId { get; set; }
    public string? PermissionId { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}
