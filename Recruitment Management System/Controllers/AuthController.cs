using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Recruitment_Management_System.Data;
using Recruitment_Management_System.DTOs.Authentication;
using Recruitment_Management_System.Models;

namespace Recruitment_Management_System.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponseDto>> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var fullName = request.FullName.Trim();
        var email = NormalizeEmail(request.Email);
        var requestedRole = string.IsNullOrWhiteSpace(request.RequestedRole)
            ? "Candidate"
            : request.RequestedRole.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            ModelState.AddModelError(nameof(request.FullName), "FullName is required.");
        }

        if (requestedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(request.RequestedRole), "Admin accounts cannot be created through public registration.");
        }

        var recruiterRegistrationAllowed = configuration.GetValue<bool>(
            "Authentication:AllowRecruiterRegistration");

        if (requestedRole.Equals("Recruiter", StringComparison.OrdinalIgnoreCase)
            && !recruiterRegistrationAllowed)
        {
            ModelState.AddModelError(
                nameof(request.RequestedRole),
                "Recruiter self-registration is disabled. Recruiter accounts must be provisioned by an administrator.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var emailExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Email.ToUpper() == email, cancellationToken);

        if (emailExists)
        {
            return Conflict(new { message = "An account with this email is already registered." });
        }

        var user = new User
        {
            FullName = fullName,
            Email = email,
            Role = requestedRole
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "An account with this email is already registered." });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            ToUserResponse(user));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.Email.ToUpper() == email, cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var response = CreateAuthResponse(user);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);

        return user is null
            ? Unauthorized()
            : Ok(ToUserResponse(user));
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var accessTokenMinutes = configuration.GetValue("Jwt:AccessTokenMinutes", 30);
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(accessTokenMinutes);
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAtUtc,
            ToUserResponse(user));
    }

    private static UserResponseDto ToUserResponse(User user) =>
        new(user.Id, user.FullName, user.Email, user.Role);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToUpperInvariant();
}
