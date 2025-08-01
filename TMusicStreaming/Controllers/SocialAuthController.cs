using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TMusicStreaming.Data;
using TMusicStreaming.Models;
using TMusicStreaming.Services.Interfaces;

namespace TMusicStreaming.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialAuthController : ControllerBase
    {
        private readonly TMusicStreamingContext _context;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SocialAuthController> _logger;
        private readonly string _frontendUrl;

        public SocialAuthController(
            TMusicStreamingContext context,
            IAuthService authService,
            IConfiguration configuration,
            ILogger<SocialAuthController> logger)
        {
            _context = context;
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
            _frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
        }

        [HttpGet("google")]
        public IActionResult GoogleLogin(string returnUrl = "")
        {
            try
            {
                // Tạo absolute URL cho callback
                var callbackUrl = GetAbsoluteUrl("api/socialauth/google-callback");

                var properties = new AuthenticationProperties
                {
                    RedirectUri = callbackUrl,
                    Items =
                    {
                        ["returnUrl"] = string.IsNullOrEmpty(returnUrl) ? $"{_frontendUrl}/watch" : returnUrl,
                        ["scheme"] = GoogleDefaults.AuthenticationScheme
                    }
                };

                _logger.LogInformation("Initiating Google login");
                _logger.LogInformation("Callback URL: {CallbackUrl}", callbackUrl);
                _logger.LogInformation("Return URL: {ReturnUrl}", returnUrl);

                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Google login");
                return Redirect($"{_frontendUrl}/auth/callback?status=error&message=" +
                    Uri.EscapeDataString("Không thể khởi tạo đăng nhập Google"));
            }
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                _logger.LogInformation("Google callback received - Request Path: {Path}", Request.Path);
                _logger.LogInformation("Google callback received - Query: {Query}", Request.QueryString);

                var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    _logger.LogError("Google authentication failed: {Error}", result.Failure?.Message);
                    _logger.LogError("Authentication result properties: {Properties}",
                        result.Properties?.Items?.Count ?? 0);

                    return Redirect($"{_frontendUrl}/auth/callback?status=error&message=" +
                        Uri.EscapeDataString("Đăng nhập Google thất bại"));
                }

                var claims = result.Principal.Claims;
                var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var avatar = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

                _logger.LogInformation("Google claims - ID: {GoogleId}, Email: {Email}, Name: {Name}",
                    googleId, email, name);

                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                {
                    _logger.LogError("Missing required claims from Google");
                    return Redirect($"{_frontendUrl}/auth/callback?status=error&message=" +
                        Uri.EscapeDataString("Không thể lấy thông tin từ Google"));
                }

                var user = await ProcessSocialUser(googleId, email, name, avatar, "Google");
                var token = _authService.GenerateToken(user);

                await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps, // Tự động detect HTTPS
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Path = "/"
                };

                Response.Cookies.Append("access_token", token, cookieOptions);

                var returnUrl = result.Properties?.Items.ContainsKey("returnUrl") == true
                    ? result.Properties.Items["returnUrl"]
                    : $"{_frontendUrl}/watch";

                _logger.LogInformation("Google login successful, redirecting to frontend callback");

                return Redirect($"{_frontendUrl}/auth/callback?status=success&message=" +
                    Uri.EscapeDataString("Đăng nhập thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Google callback");

                try
                {
                    await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);
                }
                catch { }

                return Redirect($"{_frontendUrl}/auth/callback?status=error&message=" +
                    Uri.EscapeDataString("Đã xảy ra lỗi trong quá trình đăng nhập"));
            }
        }

        [HttpGet("facebook")]
        public IActionResult FacebookLogin(string returnUrl = "")
        {
            try
            {
                var properties = new AuthenticationProperties
                {
                    RedirectUri = Url.Action("FacebookCallback", "SocialAuth"),
                    Items = {
                        ["returnUrl"] = string.IsNullOrEmpty(returnUrl) ? $"{_frontendUrl}/watch" : returnUrl,
                        ["scheme"] = FacebookDefaults.AuthenticationScheme
                    }
                };

                _logger.LogInformation("Facebook login initiated with returnUrl: {ReturnUrl}", returnUrl);
                return Challenge(properties, FacebookDefaults.AuthenticationScheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Facebook login");
                return RedirectToFrontend("error", "Không thể khởi tạo đăng nhập Facebook");
            }
        }

        [HttpGet("facebook-callback")]
        public async Task<IActionResult> FacebookCallback()
        {
            try
            {
                _logger.LogInformation("Facebook callback received");

                var result = await HttpContext.AuthenticateAsync(FacebookDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    _logger.LogError("Facebook authentication failed: {Error}", result.Failure?.Message);
                    return RedirectToFrontend("error", "Đăng nhập Facebook thất bại");
                }

                var claims = result.Principal.Claims;
                var facebookId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var avatar = $"https://graph.facebook.com/{facebookId}/picture?type=large";

                _logger.LogInformation("Facebook claims - ID: {FacebookId}, Email: {Email}, Name: {Name}",
                    facebookId, email, name);

                if (string.IsNullOrEmpty(facebookId))
                {
                    _logger.LogError("Missing Facebook ID");
                    return RedirectToFrontend("error", "Không thể lấy thông tin từ Facebook");
                }

                // Facebook có thể không trả về email
                if (string.IsNullOrEmpty(email))
                {
                    email = $"{facebookId}@facebook.local";
                }

                var user = await ProcessSocialUser(facebookId, email, name, avatar, "Facebook");
                var token = _authService.GenerateToken(user);

                // Cleanup
                await HttpContext.SignOutAsync(FacebookDefaults.AuthenticationScheme);

                // Set JWT token cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    Path = "/"
                };

                Response.Cookies.Append("access_token", token, cookieOptions);

                _logger.LogInformation("Facebook login successful for user: {FacebookId}", facebookId);
                return RedirectToFrontend("success", "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Facebook callback");

                // Cleanup
                try
                {
                    await HttpContext.SignOutAsync(FacebookDefaults.AuthenticationScheme);
                }
                catch { }

                return RedirectToFrontend("error", "Đã xảy ra lỗi trong quá trình đăng nhập");
            }
        }

        // Thêm method test để kiểm tra routing
        [HttpGet("test")]
        public IActionResult Test()
        {
            _logger.LogInformation("SocialAuth Test endpoint called");
            return Ok(new { message = "SocialAuth controller is working", timestamp = DateTime.UtcNow });
        }

        private async Task<User> ProcessSocialUser(string platformId, string email, string name, string avatar, string platform)
        {
            // Tìm user theo PlatformId và PlatformName
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PlatformId == platformId && u.PlatformName == platform);

            if (user != null)
            {
                // Cập nhật thông tin nếu có thay đổi
                user.Name = name ?? user.Name;
                user.Avatar = avatar ?? user.Avatar;
                user.Status = 1; // Active
                await _context.SaveChangesAsync();
                return user;
            }

            // Kiểm tra email đã tồn tại với tài khoản thường
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null && string.IsNullOrEmpty(existingUser.PlatformId))
            {
                // Link tài khoản social với tài khoản hiện tại
                existingUser.PlatformId = platformId;
                existingUser.PlatformName = platform;
                existingUser.Avatar = avatar ?? existingUser.Avatar;
                existingUser.Status = 1;
                await _context.SaveChangesAsync();
                return existingUser;
            }

            // Tạo user mới
            var newUser = new User
            {
                UserName = GenerateUsername(email, name),
                Name = name ?? "User",
                Email = email,
                Avatar = avatar ?? "",
                Gender = false, // Default
                Role = "User",
                PlatformId = platformId,
                PlatformName = platform,
                PasswordHash = "", // Không cần password cho social login
                Status = 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }

        private string GenerateUsername(string email, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name.Replace(" ", "").ToLower() + new Random().Next(1000, 9999);
            }

            return email.Split('@')[0] + new Random().Next(1000, 9999);
        }

        private IActionResult RedirectToFrontend(string status, string message)
        {
            var frontendUrl = _frontendUrl;
            var redirectUrl = $"{frontendUrl}/auth/callback?status={status}&message={Uri.EscapeDataString(message)}";

            _logger.LogInformation("Redirecting to frontend: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }

        [HttpGet("check-auth")]
        public async Task<IActionResult> CheckAuth()
        {
            try
            {
                var token = Request.Cookies["access_token"];
                if (string.IsNullOrEmpty(token))
                {
                    return Ok(new { isAuthenticated = false });
                }

                var principal = _authService.ValidateToken(token);
                if (principal == null)
                {
                    return Ok(new { isAuthenticated = false });
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users
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
                    return Ok(new { isAuthenticated = false });
                }

                return Ok(new
                {
                    isAuthenticated = true,
                    user = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auth status");
                return Ok(new { isAuthenticated = false });
            }
        }

        private string GetAbsoluteUrl(string relativePath)
        {
            var request = HttpContext.Request;
            var scheme = request.Headers.ContainsKey("X-Forwarded-Proto")
                ? request.Headers["X-Forwarded-Proto"].ToString()
                : request.Scheme;

            var host = request.Headers.ContainsKey("X-Forwarded-Host")
                ? request.Headers["X-Forwarded-Host"].ToString()
                : request.Host.Value;

            return $"{scheme}://{host}/{relativePath}";
        }

        [HttpGet("debug-request")]
        public IActionResult DebugRequest()
        {
            var request = HttpContext.Request;

            // Parse Cf-Visitor header nếu có
            string detectedScheme = "unknown";
            if (request.Headers.ContainsKey("Cf-Visitor"))
            {
                var cfVisitor = request.Headers["Cf-Visitor"].ToString();
                detectedScheme = cfVisitor.Contains("\"scheme\":\"https\"") ? "https" : "http";
            }

            var info = new
            {
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                RequestScheme = request.Scheme,
                RequestIsHttps = request.IsHttps,
                DetectedScheme = detectedScheme,
                Host = request.Host.Value,
                Path = request.Path,
                QueryString = request.QueryString.Value,

                // Render-specific headers
                RenderHeaders = new
                {
                    CfVisitor = request.Headers.ContainsKey("Cf-Visitor") ? request.Headers["Cf-Visitor"].ToString() : null,
                    XOriginalProto = request.Headers.ContainsKey("X-Original-Proto") ? request.Headers["X-Original-Proto"].ToString() : null,
                    RenderProxyTtl = request.Headers.ContainsKey("Render-Proxy-Ttl") ? request.Headers["Render-Proxy-Ttl"].ToString() : null
                },

                // All headers for debugging
                AllHeaders = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),

                // Generated URLs
                AbsoluteCallbackUrl = GetAbsoluteUrl("api/socialauth/google-callback"),
                FrontendUrl = _configuration["FrontendUrl"]
            };

            _logger.LogInformation("Request debug info: {@Info}", info);
            return Ok(info);
        }
    }
}