using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.Models.Posts;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public IActionResult Create(CreatePostRequest model)
        {
            var userId = GetUserIdFromToken();
            _postService.Create(model, userId);
            return Ok(new { message = "Post created successfully" });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            var posts = _postService.GetAllWithUser();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var post = _postService.GetById(id);
            return Ok(post);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdatePostRequest model)
        {
            var userId = GetUserIdFromToken();
            _postService.Update(id, model, userId);
            return Ok(new { message = "Post updated successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = GetUserIdFromToken();
            _postService.Delete(id, userId);
            return Ok(new { message = "Post deleted successfully" });
        }

        [HttpPost("{postId}/files")]
        public IActionResult AddFileToPost(int postId, [FromBody] PostFileRequest model)
        {
            _postService.AddFileToPost(postId, model);
            return Ok(new { message = "File added successfully" });
        }

        [HttpDelete("{postId}/files/{fileId}")]
        public IActionResult RemoveFileFromPost(int postId, int fileId)
        {
            _postService.RemoveFileFromPost(postId, fileId);
            return Ok(new { message = "File removed successfully" });
        }

        [AllowAnonymous]
        [HttpGet("{postId}/files/{fileId}")]
        public IActionResult GetFile(int postId, int fileId)
        {
            var file = _postService.GetFile(postId, fileId);
            return File(file.FileContent, file.FileType, file.FileName);
        }

        [AllowAnonymous]
        [HttpGet("filter")]
        public IActionResult GetFilteredPosts([FromQuery] string fileType, [FromQuery] string searchTerm)
        {
            var posts = _postService.GetFilteredPosts(fileType, searchTerm);
            return Ok(posts);
        }

        private int GetUserIdFromToken()
        {
            var user = (Api.Entities.User)HttpContext.Items["User"];
            var userId = user?.Id ?? 0;
            return userId;
        }
    }
}
