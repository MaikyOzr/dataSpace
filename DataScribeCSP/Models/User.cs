using Microsoft.AspNetCore.Identity;

namespace DataScribeCSP.Models
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Files> Files { get; set; } = new List<Files>();
    }
}
