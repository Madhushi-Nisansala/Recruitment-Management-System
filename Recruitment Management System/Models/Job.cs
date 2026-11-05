using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Recruitment_Management_System.Models;

public sealed class Job : IValidatableObject
{
    public int Id { get; set; }

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

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosingDate { get; set; }

    public int RecruiterId { get; set; }

    [JsonIgnore]
    public User Recruiter { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalaryMin.HasValue && SalaryMax.HasValue && SalaryMin > SalaryMax)
        {
            yield return new ValidationResult(
                "SalaryMin must be less than or equal to SalaryMax.",
                [nameof(SalaryMin), nameof(SalaryMax)]);
        }

        if (ClosingDate.HasValue && ClosingDate <= PostedAt)
        {
            yield return new ValidationResult(
                "ClosingDate must be later than PostedAt.",
                [nameof(ClosingDate)]);
        }
    }
}
