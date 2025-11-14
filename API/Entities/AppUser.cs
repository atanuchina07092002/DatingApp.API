using API.Extensions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class AppUser:IdentityUser<int> //int for integer type id
    {
        public DateOnly DateOfBirth { get; set; }
        public required string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public required string Gender { get; set; }
        public string? Introduction { get; set; }
        public string? LookingFor { get; set; }
        public string? Interests { get; set; }
        public required string City { get; set; }
        public required string Country { get; set; }
        public List<Photo> photos { get; set; } = [];
        public List<UserLike> LikedUsers { get; set; } = [];
        public List<UserLike> LikedByUsers { get; set; } = [];
        public List<Message> MessagesSent { get; set; } = [];
        public List<Message> MessagesReceived { get; set; } = [];
        public ICollection<AppUsersRole> UsersRoles { get; set; }



    }
}
