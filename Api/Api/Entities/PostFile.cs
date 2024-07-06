namespace Api.Entities
{
    public class PostFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileContent { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public string FileUrl { get; set; } // Dodane pole na URL pliku
    }
}
