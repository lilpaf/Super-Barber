using Microsoft.AspNetCore.Identity;

namespace SuperBarber.Data.Models
{
    public class User : IdentityUser
    {
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
