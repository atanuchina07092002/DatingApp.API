using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class UserDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? KnownAs { get; set; }
        [Required]
        public string? Token { get; set; }
        [Required]
        public string? photoUrl { get; set; }
        public string? Gender { get; set; }
    }
}
