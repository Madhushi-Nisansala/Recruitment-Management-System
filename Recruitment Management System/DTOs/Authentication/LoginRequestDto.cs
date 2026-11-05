using System.ComponentModel.DataAnnotations;

namespace Recruitment_Management_System.DTOs.Authentication;

public sealed class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
