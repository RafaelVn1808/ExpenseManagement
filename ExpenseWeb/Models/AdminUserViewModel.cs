using System.Collections.Generic;
using System.Linq;

namespace ExpenseWeb.Models
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();

        public bool IsAdmin => Roles.Any(role => role.Equals("Admin", System.StringComparison.OrdinalIgnoreCase));
        public bool IsUser => Roles.Any(role => role.Equals("User", System.StringComparison.OrdinalIgnoreCase));
    }
}
