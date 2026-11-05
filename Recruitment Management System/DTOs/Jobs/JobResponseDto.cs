namespace Recruitment_Management_System.DTOs.Jobs;

public sealed record JobResponseDto(
    int Id,
    string Title,
    string Description,
    string? Location,
    string? EmploymentType,
    decimal? SalaryMin,
    decimal? SalaryMax,
    DateTime PostedAt,
    DateTime? ClosingDate,
    RecruiterSummaryDto Recruiter);

public sealed record RecruiterSummaryDto(
    int Id,
    string FullName,
    string Email);
