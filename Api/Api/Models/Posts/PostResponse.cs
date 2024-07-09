namespace Api.Models.Posts
{
    public class PostResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int UserId { get; set; }
        public UserResponse User { get; set; }
        public List<PostFileResponse> Files { get; set; }
        public int Likes { get; set; } // Dodane pole dla like'ów
        public int Dislikes { get; set; } // Dodane pole dla dislike'ów
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
    }
}
