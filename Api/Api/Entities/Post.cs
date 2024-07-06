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
    }
}
