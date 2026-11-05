using System.ComponentModel.DataAnnotations;

namespace Recruitment_Management_System.DTOs.Applications;

public sealed class UpdateApplicationStatusDto
{
    [Required]
    [MaxLength(30)]
    [RegularExpression("^(Pending|Reviewed|Accepted|Rejected|Withdrawn)$")]
    public string Status { get; set; } = string.Empty;
}
