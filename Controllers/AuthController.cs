using BlogApi.Data;
using BlogApi.DTOs.User;
using BlogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BlogApi.Services;
using BlogApi.DTOs;

namespace BlogApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BloggingContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IApiLogger _logger;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthController(BloggingContext context, IWebHostEnvironment env, IApiLogger logger, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _config = config;
            _emailService = emailService;
        }

        /// <summary>
        /// This api is to add a new user for login.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("SignUp")]
        public async Task<IActionResult> Register([FromForm] UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
                return BadRequest("Username already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                PasswordHash = passwordHash,
                Website = dto.Website,
                EmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString("N"),
                EmailConfirmationTokenExpires = DateTime.UtcNow.AddHours(24)

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

            var verifyUrl =
                $"{_config["App:BaseUrl"]}/api/auth/verify-email?token={user.EmailConfirmationToken}";

            await _emailService.SendAsync(
                user.Email,
                "Verify your email",
                $"Click this link to verify your email:\n{verifyUrl}"
            );

            return Ok($"To login please click the verification link sent to {dto.Email}.");
        }

        /// <summary>
        /// Controller to verify token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.EmailConfirmationToken == token);

            if (user == null)
                return BadRequest("User doesn't exist or Invalid verification token or Expired verification token. Please sign up again");
                
            if (user.EmailConfirmationTokenExpires < DateTime.UtcNow)
            {
                _context.Users.Remove(user);
                return BadRequest("User doesn't exist or Invalid verification token or Expired verification token. Please sign up again");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Email verified successfully.");
        }


        /// <summary>
        /// Api to log into the system.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid username or password");

            if (!user.EmailConfirmed)
                return Unauthorized("Please verify your email first.");

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
                new Claim(ClaimTypes.Role, user.Role.ToString()),
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