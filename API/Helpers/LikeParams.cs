namespace API.Helpers
{
    public class LikeParams:PaginationPrams
    {
        public int UserId { get; set; }
        public string Predicate { get; set; } = "liked";
    }
}
