namespace Recruitment_Management_System.DTOs.Authentication;

public sealed record AuthResponseDto(
    string AccessToken,
    string TokenType,
    DateTime ExpiresAtUtc,
    UserResponseDto User);
