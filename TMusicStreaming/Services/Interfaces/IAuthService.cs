using System.Security.Claims;
using TMusicStreaming.Models;

namespace TMusicStreaming.Services.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
