namespace Api.Models.Posts
{
    public class PostLikeDislikeRequest
    {
        public int PostId { get; set; }
        public int UserId { get; set; } // Dodane pole UserId
    }
}
