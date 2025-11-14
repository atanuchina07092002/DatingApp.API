using System.ComponentModel.DataAnnotations;

namespace API.DTO
{
    public class CreateMessageDto
    {
        [Required]
        public required string RecipientUsername { get; set; }
        [Required]
        public required string Content { get; set; }
    }
}
