using Microsoft.AspNetCore.Identity;

namespace Server.Models
{
    public class AppUser:IdentityUser
    {
        public string? FullName { get; set; }
    }
}