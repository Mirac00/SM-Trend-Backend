using System.Collections.Generic;

namespace Api.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<PostFile> Files { get; set; } = new List<PostFile>();
        public int Likes { get; set; } // Dodane pole dla like'ów
        public int Dislikes { get; set; } // Dodane pole dla dislike'ów
    }
}
