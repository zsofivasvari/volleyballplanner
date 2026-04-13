using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Auth;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        AppDbContext context,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
        {
            throw new Exception("Ez az email cím már használatban van.");
        }

        var emailConfirmationToken = Guid.NewGuid().ToString();

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsEmailConfirmed = false,
            EmailConfirmationToken = emailConfirmationToken,
            EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var confirmationLink = $"{frontendBaseUrl}/confirm-email?token={emailConfirmationToken}";

        var htmlBody = $@"
            <h2>Email megerősítés</h2>
            <p>Kedves {user.Name}!</p>
            <p>Kérlek erősítsd meg az email címedet az alábbi linkre kattintva:</p>
            <p><a href='{confirmationLink}'>Email cím megerősítése</a></p>
            <p>A link 24 óráig érvényes.</p>
        ";

        await _emailService.SendEmailAsync(
            user.Email,
            "VolleyMind - Email megerősítés",
            htmlBody);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var profile = new UserProfile
        {
            UserId = user.Id
        };

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return new RegisterResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (!user.IsEmailConfirmed)
            {
                throw new Exception("Az email cím még nincs megerősítve.");
            }
        
        if (user == null)
        {
            throw new Exception("Hibás email vagy jelszó.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new Exception("Hibás email vagy jelszó.");
        }

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Name = user.Name,
            Email = user.Email
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<UserProfileResponseDto> GetCurrentUserProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("A felhasználó nem található.");
        }

        return new UserProfileResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Level = user.Profile?.Level,
            Goal = user.Profile?.Goal,
            Age = user.Profile?.Age,
            Height = user.Profile?.Height,
            Weight = user.Profile?.Weight
        };
    }

    public async Task<bool> ConfirmEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.EmailConfirmationToken == token &&
                u.EmailConfirmationTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
            {
                return false;
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiresAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return;
            }

            var resetToken = Guid.NewGuid().ToString();

            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontendBaseUrl}/reset-password?token={resetToken}";

            var htmlBody = $@"
                <h2>Jelszó visszaállítás</h2>
                <p>Kedves {user.Name}!</p>
                <p>Az alábbi linkre kattintva új jelszót adhatsz meg:</p>
                <p><a href='{resetLink}'>Jelszó visszaállítása</a></p>
                <p>A link 1 óráig érvényes.</p>
            ";

            await _emailService.SendEmailAsync(
                user.Email,
                "VolleyMind - Jelszó visszaállítás",
                htmlBody);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token &&
                u.PasswordResetTokenExpiresAt > DateTime.UtcNow);

            if (user == null)
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresAt = null;

            await _context.SaveChangesAsync();
            return true;
        }

}