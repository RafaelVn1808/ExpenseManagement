using System.Collections.Generic;

namespace ExpenseWeb.Models
{
    public class UpdateUserRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
