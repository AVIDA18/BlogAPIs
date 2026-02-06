using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using BlogApi.Models;
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
        private readonly IWebHostEnvironment _env;
        private readonly IApiLogger _logger;

        public UserController(BloggingContext context, IWebHostEnvironment env, IApiLogger logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// This api is to add a new user for login.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
                return BadRequest("Username already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                UserName = dto.UserName,
                PasswordHash = passwordHash,
                Email = dto.Email,
                Website = dto.Website

            };

            if (dto.ProfileImage != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ProfileImage.FileName);
                var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "Profile", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfileImage.CopyToAsync(stream);
                }

                user.ProfileImagePath = "/Profile/" + fileName;
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
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