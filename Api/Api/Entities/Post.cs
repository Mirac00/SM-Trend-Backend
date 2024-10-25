namespace Api.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; } // Dodane pole kategorii
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<PostFile> Files { get; set; } = new List<PostFile>();
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}
