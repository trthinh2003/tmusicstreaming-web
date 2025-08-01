using Microsoft.AspNetCore.Mvc;
using TMusicStreaming.Data;
using TMusicStreaming.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TMusicStreaming.DTOs.Auth;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly TMusicStreamingContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, TMusicStreamingContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _context.Users!.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
                }

                var token = _authService.GenerateToken(user);

                // Lưu JWT vào cookie
                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });
                return Ok(new { 
                    message = "Đăng nhập thành công!" ,
                });
            } catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while logging in", error = ex.ToString() });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (_context.Users!.Any(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "Email này đã tồn tại" });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    UserName = request.Email.Split('@')[0],
                    Name = request.Name,
                    Email = request.Email,
                    Gender = request.Gender,
                    PasswordHash = hashedPassword,
                    Role = "User"
                };

                _context.Users!.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while registering the user", error = ex.Message });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                if (!User.Identity!.IsAuthenticated)
                {
                    return Unauthorized(new { message = "Unauthorized" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy userId trong token" });
                }

                var user = await _context.Users!
                    .Where(u => u.Id.ToString() == userId)
                    .Select(u => new
                    {
                        id = u.Id,
                        username = u.UserName,
                        email = u.Email,
                        role = u.Role,
                        avatar = u.Avatar
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng!" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while getting profile",
                    error = ex.Message
                });
            }
        }


        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            try
            {
                var token = Request.Cookies["access_token"];
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Token không hợp lệ!" });
                }

                // Xác thực token cũ
                var principal = _authService.ValidateToken(token);
                if (principal == null)
                {
                    return Unauthorized(new { message = "Token hết hạn hoặc không hợp lệ!" });
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = _context.Users!.FirstOrDefault(u => u.Id.ToString() == userId);
                if (user == null)
                {
                    return Unauthorized(new { message = "Không tìm thấy người dùng!" });
                }

                // sinh 1 token mới
                var newToken = _authService.GenerateToken(user);

                Response.Cookies.Append("access_token", newToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });

                return Ok(new { message = "Token đã được làm mới thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi refresh token", error = ex.Message });
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            return Ok(new { message = "Đăng xuất thành công!" });
        }
    }
}
