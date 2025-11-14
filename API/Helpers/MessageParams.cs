namespace API.Helpers
{
    public class MessageParams : PaginationPrams
    {
        public string? Username { get; set; }
        public string Container { get; set; } = "Unread";
    }
}
