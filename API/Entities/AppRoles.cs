using Microsoft.AspNetCore.Identity;

namespace API.Entities
{
    public class AppRoles:IdentityRole<int>
    {
        public ICollection<AppUsersRole> UsersRoles { get; set; } = [];
    }
}
