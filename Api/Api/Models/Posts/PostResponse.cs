namespace Api.Models.Posts
{
    public class PostResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; } // Dodane pole kategorii
        public int UserId { get; set; }
        public UserResponse User { get; set; }
        public List<PostFileResponse> Files { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
    }
}
