namespace Api.Entities
{
    public class PostLikeDislike
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsLike { get; set; } // true = like, false = dislike
    }
}
