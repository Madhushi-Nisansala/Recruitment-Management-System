using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Recruitment_Management_System.Models;

public sealed class Application
{
    public int Id { get; set; }

    public int CandidateId { get; set; }

    public int JobId { get; set; }

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    [MaxLength(2000)]
    public string? CoverLetter { get; set; }

    [MaxLength(500)]
    public string? CvFilePath { get; set; }

    [JsonIgnore]
    public User Candidate { get; set; } = null!;

    [JsonIgnore]
    public Job Job { get; set; } = null!;
}
