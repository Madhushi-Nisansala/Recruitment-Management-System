using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Recruitment_Management_System.Models;

public sealed class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    [RegularExpression("^(Candidate|Recruiter|Admin)$")]
    public string Role { get; set; } = "Candidate";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    [JsonIgnore]
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
