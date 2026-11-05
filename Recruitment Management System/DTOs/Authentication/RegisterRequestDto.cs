using System.ComponentModel.DataAnnotations;

namespace Recruitment_Management_System.DTOs.Authentication;

public sealed class RegisterRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(30)]
    [RegularExpression("^(Candidate|Recruiter)$")]
    public string? RequestedRole { get; set; }
}
