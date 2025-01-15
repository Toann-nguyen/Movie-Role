using MvcMovie.ViewModels;
namespace MvcMovie.ViewModels
{
    public class RoleViewModel
    {
        public string? RoleId { get; set; } = string.Empty;
        public string? RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}