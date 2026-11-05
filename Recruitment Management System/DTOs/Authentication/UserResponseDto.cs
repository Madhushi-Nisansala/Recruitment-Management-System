namespace Recruitment_Management_System.DTOs.Authentication;

public sealed record UserResponseDto(
    int Id,
    string FullName,
    string Email,
    string Role);
