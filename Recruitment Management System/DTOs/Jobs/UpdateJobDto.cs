using System.ComponentModel.DataAnnotations;

namespace Recruitment_Management_System.DTOs.Jobs;

public sealed class UpdateJobDto : IValidatableObject
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Location { get; set; }

    [MaxLength(50)]
    public string? EmploymentType { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }

    public DateTime? ClosingDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryMin.HasValue && SalaryMax.HasValue && SalaryMin > SalaryMax)
        {
            yield return new ValidationResult(
                "SalaryMin must be less than or equal to SalaryMax.",
                [nameof(SalaryMin), nameof(SalaryMax)]);
        }
    }
}
