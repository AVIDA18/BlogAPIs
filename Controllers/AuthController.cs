using BlogApi.Data;
using BlogApi.DTOs.User;
using BlogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using BlogApi.Services;
using System.Text.Json;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IApiLogger _logger;

        public AuthController(BloggingContext context, IConfiguration config, IWebHostEnvironment env, IApiLogger logger)
        {
            _context = context;
            _config = config;
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
                var filePath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfileImage.CopyToAsync(stream);
                }

                user.ProfileImagePath = "/uploads/" + fileName;
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
                    Role = u.Role,
                    Website = u.Website
                })
                .ToListAsync();

            return Ok(users);
        }


        /// <summary>
        /// Api to log into the system.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            var token = GenerateJwtToken(user);

            await _logger.LogAsync(
                api: "/api/Auth/login",
                payload: dto.UserName,
                response: token,
                userId: Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "id")?.Value)
                );

            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("username", user.UserName.ToString()),
                new Claim("role", user.Role.ToString()),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}