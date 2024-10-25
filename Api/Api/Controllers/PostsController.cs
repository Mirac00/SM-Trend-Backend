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

        [AllowAnonymous]
        [HttpGet("filtered")]
        public IActionResult GetFilteredPosts([FromQuery] string fileType, [FromQuery] string searchTerm)
        {
            var posts = _postService.GetFilteredPosts(fileType, searchTerm);
            return Ok(posts);
        }

        [HttpGet("user/{userId:int}")]
        public IActionResult GetPostsByUser(int userId)
        {
            var posts = _postService.GetPostsByUser(userId);
            return Ok(posts);
        }

        [HttpGet("liked/{userId:int}")]
        public IActionResult GetLikedPostsByUser(int userId)
        {
            var posts = _postService.GetLikedPostsByUser(userId);
            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
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

        [AllowAnonymous]
        [HttpGet("{postId}/files/{fileId}/content")]
        public IActionResult GetFileContent(int postId, int fileId)
        {
            var file = _postService.GetFile(postId, fileId);
            return File(file.FileContent, file.FileType);
        }

        [HttpGet("{postId}/files/{fileId}/download")]
        public IActionResult DownloadFile(int postId, int fileId)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized();

            var file = _postService.GetFile(postId, fileId);
            return File(file.FileContent, file.FileType, file.FileName);
        }

        [HttpPost("{postId}/like")]
        public IActionResult LikePost(int postId)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized();

            _postService.LikePost(new PostLikeDislikeRequest { PostId = postId, UserId = userId });
            return Ok(new { message = "Post liked successfully" });
        }

        [HttpPost("{postId}/dislike")]
        public IActionResult DislikePost(int postId)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized();

            _postService.DislikePost(new PostLikeDislikeRequest { PostId = postId, UserId = userId });
            return Ok(new { message = "Post disliked successfully" });
        }

        [HttpGet("{postId}/like-status")]
        public IActionResult GetUserLikeStatus(int postId)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized();

            var isLike = _postService.GetUserLikeStatus(postId, userId);
            return Ok(new { isLike });
        }

        [AllowAnonymous]
        [HttpGet("{postId}/files/{fileId}/thumbnail")]
        public IActionResult GetFileThumbnail(int postId, int fileId)
        {
            var file = _postService.GetFile(postId, fileId);
            if (file.FileType.StartsWith("image/"))
            {
                return File(file.FileContent, file.FileType);
            }
            else
            {
                // Możesz zwrócić domyślną miniaturkę dla innych typów plików
                return NotFound();
            }
        }

        // Dodany endpoint do usuwania pliku z posta
        [HttpDelete("{postId}/files/{fileId}")]
        public IActionResult RemoveFileFromPost(int postId, int fileId)
        {
            var userId = GetUserIdFromToken();
            if (userId == 0)
                return Unauthorized();

            try
            {
                _postService.RemoveFileFromPost(postId, fileId, userId);
                return Ok(new { message = "File removed from post successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Dodany endpoint do pobierania top 10 najbardziej lajkowanych postów
        [AllowAnonymous]
        [HttpGet("top-liked")]
        public IActionResult GetTopLikedPosts()
        {
            var posts = _postService.GetTopLikedPosts();
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
