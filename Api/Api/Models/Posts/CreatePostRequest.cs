namespace Api.Models.Posts
{
    public class CreatePostRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; } // Dodane pole kategorii
        public PostFileRequest File { get; set; } // Zmienione z List<PostFileRequest> na PostFileRequest
    }
}
