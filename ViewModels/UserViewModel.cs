
using MvcMovie.Models;

namespace MvcMovie.ViewModels
{
    public class UserViewModel
    {
        public ApplicationUser? User { get; set; }
        public IEnumerable<string>? Roles { get; set; }
        public IEnumerable<Permission>? Permissions { get; set; }
    }
}