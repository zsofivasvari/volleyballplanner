using VolleyPlanner.API.DTOs.Auth;

namespace VolleyPlanner.API.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserProfileResponseDto> GetCurrentUserProfileAsync(int userId);

    Task<bool> ConfirmEmailAsync(string token);
    Task ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}