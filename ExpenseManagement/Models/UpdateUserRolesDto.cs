using System.Collections.Generic;

namespace ExpenseApi.Models
{
    public class UpdateUserRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }
}
