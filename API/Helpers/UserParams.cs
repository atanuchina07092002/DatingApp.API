namespace API.Helpers
{
    public class UserParams:PaginationPrams
    {
        public string? Gender { get; set; }
        public string? UserName { get; set; }
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 60;
        public string OrderBy { get; set; } = "lastActive";

    }
}
//Create the Pagination Parameters Class