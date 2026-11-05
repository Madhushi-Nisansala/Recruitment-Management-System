namespace Recruitment_Management_System.DTOs.Applications;

public sealed record ApplicationResponseDto(
    int Id,
    int CandidateId,
    string CandidateName,
    int JobId,
    string JobTitle,
    int RecruiterId,
    string RecruiterName,
    DateTime AppliedAt,
    string Status,
    string? CoverLetter,
    string? CvFilePath);
