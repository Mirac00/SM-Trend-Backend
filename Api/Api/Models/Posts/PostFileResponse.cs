namespace Api.Models.Posts
{
    public class PostFileResponse
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public int PostId { get; set; }
    }
}
