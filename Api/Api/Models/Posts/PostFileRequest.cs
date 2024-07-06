namespace Api.Models.Posts
{
    public class PostFileRequest
    {
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileContent { get; set; } // Base64 encoded content
    }
}
