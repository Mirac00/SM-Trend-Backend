// UsersController.cs
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api.Helpers;
using Api.Models.Users;
using Api.Services;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IPostService _postService;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            IPostService postService)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _postService = postService;
        }

        [HttpGet("GetUserByToken")]
        public IActionResult GetUserByToken()
        {
            var user = _userService.GetUserByToken(HttpContext.Request.Headers["Authorization"]);
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            var response = _userService.Authenticate(model);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _userService.Register(model);
            return Ok(new { message = "Registration successful" });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateRequest model)
        {
            _userService.Update(id, model);
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPut("{id}/update-profile")]
        public IActionResult UpdateProfile(int id, UpdateRequest model)
        {
            _userService.Update(id, model);
            return Ok(new { message = "Profile updated successfully" });
        }

        // Nowy endpoint do odświeżania tokenu
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public IActionResult RefreshToken()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
                return BadRequest(new { message = "Token is required" });

            var token = authHeader.Split(" ").Last();
            var newToken = _userService.RefreshToken(token);

            if (string.IsNullOrEmpty(newToken))
                return Unauthorized(new { message = "Invalid token" });

            return Ok(new { token = newToken });
        }
    }
}
