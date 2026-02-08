using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApi.Data;
using BlogApi.DTOs.User;
using System.Security.Claims;
using BlogApi.Services;
using System.Text.Json;


namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IApiLogger _logger;

        public UserController(BloggingContext context, IWebHostEnvironment env, IApiLogger logger, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// This is the api to get all the users. Only the users with role "admin" gets to use this api.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("getUsers")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// This api is to promote and demote a user. Only the user with role "admin" can use this api.
        /// </summary>
        /// <param name="userModifyDto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("ModifyUserRole")]
        public async Task<IActionResult> ModifyUserRole([FromBody] UserModifyDto userModifyDto)
        {
            var user = await _context.Users.FindAsync(userModifyDto.UserId);
            if (user == null)
                return NotFound();

            // check if current user is the commenter or Admin
            int userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value);
            string? role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Admin")
            {
                if (user.Role == "User")
                {
                    user.Role = "Admin";
                }
                else
                {
                    user.Role = "User";
                }

                await _context.SaveChangesAsync();

                await _logger.LogAsync(
                    api: "api/Auth/ModifyUser",
                    payload: JsonSerializer.Serialize(user),
                    response: "",
                    userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                    );

                return Ok(user);
            }
            else
            {
                return Forbid();
            }

        }
    }
}