
using MvcMovie.ViewModels;
namespace MvcMovie.ViewModel;

public class AssignRolesViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<RoleSelection> Roles { get; set; } = new();
}

public class RoleSelection
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool Assigned { get; set; }
}
