﻿namespace Api.Models.Posts
{
    public class UpdatePostRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<PostFileRequest> Files { get; set; }
    }
}
