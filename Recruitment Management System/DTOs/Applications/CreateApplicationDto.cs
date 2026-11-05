using System.ComponentModel.DataAnnotations;

namespace Recruitment_Management_System.DTOs.Applications;

public sealed class CreateApplicationDto
{
    [Range(1, int.MaxValue)]
    public int JobId { get; set; }

    [MaxLength(2000)]
    public string? CoverLetter { get; set; }
}
