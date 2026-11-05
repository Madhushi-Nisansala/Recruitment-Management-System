using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment_Management_System.Data;
using Recruitment_Management_System.DTOs.Applications;
using Recruitment_Management_System.Models;

namespace Recruitment_Management_System.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public sealed class ApplicationsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Candidate,Recruiter,Admin")]
    public async Task<ActionResult<IEnumerable<ApplicationResponseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var query = dbContext.Applications
            .AsNoTracking()
            .Include(application => application.Candidate)
            .Include(application => application.Job)
            .ThenInclude(job => job.Recruiter)
            .AsQueryable();

        if (User.IsInRole("Candidate"))
        {
            var candidateId = GetCurrentUserId();
            if (candidateId is null)
            {
                return Unauthorized();
            }

            query = query.Where(application => application.CandidateId == candidateId.Value);
        }
        else if (User.IsInRole("Recruiter"))
        {
            var recruiterId = GetCurrentUserId();
            if (recruiterId is null)
            {
                return Unauthorized();
            }

            query = query.Where(application => application.Job.RecruiterId == recruiterId.Value);
        }

        var applications = await query
            .OrderByDescending(application => application.AppliedAt)
            .Select(application => new ApplicationResponseDto(
                application.Id,
                application.CandidateId,
                application.Candidate.FullName,
                application.JobId,
                application.Job.Title,
                application.Job.RecruiterId,
                application.Job.Recruiter.FullName,
                application.AppliedAt,
                application.Status,
                application.CoverLetter,
                application.CvFilePath))
            .ToListAsync(cancellationToken);

        return Ok(applications);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Candidate,Recruiter,Admin")]
    public async Task<ActionResult<ApplicationResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var application = await dbContext.Applications
            .AsNoTracking()
            .Include(candidate => candidate.Candidate)
            .Include(candidate => candidate.Job)
            .ThenInclude(job => job.Recruiter)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        if (!CanAccess(application))
        {
            return Forbid();
        }

        return Ok(ToResponse(application));
    }

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<ApplicationResponseDto>> Create(
        CreateApplicationDto request,
        CancellationToken cancellationToken)
    {
        var candidateId = GetCurrentUserId();
        if (candidateId is null)
        {
            return Unauthorized();
        }

        var job = await dbContext.Jobs
            .Include(candidate => candidate.Recruiter)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.JobId, cancellationToken);

        if (job is null)
        {
            return NotFound(new { message = "The selected job does not exist." });
        }

        if (job.ClosingDate.HasValue && job.ClosingDate <= DateTime.UtcNow)
        {
            return Conflict(new { message = "Applications are closed for this job." });
        }

        var duplicate = await dbContext.Applications
            .AnyAsync(
                application => application.CandidateId == candidateId.Value
                    && application.JobId == request.JobId,
                cancellationToken);

        if (duplicate)
        {
            return Conflict(new { message = "You have already applied for this job." });
        }

        var candidate = await dbContext.Users
            .SingleAsync(user => user.Id == candidateId.Value, cancellationToken);
        var application = new Application
        {
            CandidateId = candidateId.Value,
            Candidate = candidate,
            JobId = job.Id,
            Job = job,
            AppliedAt = DateTime.UtcNow,
            Status = "Pending",
            CoverLetter = string.IsNullOrWhiteSpace(request.CoverLetter)
                ? null
                : request.CoverLetter.Trim()
        };

        dbContext.Applications.Add(application);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "You have already applied for this job." });
        }

        return CreatedAtAction(nameof(GetById), new { id = application.Id }, ToResponse(application));
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<ApplicationResponseDto>> UpdateStatus(
        int id,
        UpdateApplicationStatusDto request,
        CancellationToken cancellationToken)
    {
        var application = await dbContext.Applications
            .Include(candidate => candidate.Candidate)
            .Include(candidate => candidate.Job)
            .ThenInclude(job => job.Recruiter)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (application is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Recruiter") && GetCurrentUserId() != application.Job.RecruiterId)
        {
            return Forbid();
        }

        application.Status = request.Status.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(application));
    }

    private bool CanAccess(Application application)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var currentUserId = GetCurrentUserId();
        return User.IsInRole("Candidate")
            ? currentUserId == application.CandidateId
            : User.IsInRole("Recruiter") && currentUserId == application.Job.RecruiterId;
    }

    private int? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }

    private static ApplicationResponseDto ToResponse(Application application) =>
        new(
            application.Id,
            application.CandidateId,
            application.Candidate.FullName,
            application.JobId,
            application.Job.Title,
            application.Job.RecruiterId,
            application.Job.Recruiter.FullName,
            application.AppliedAt,
            application.Status,
            application.CoverLetter,
            application.CvFilePath);
}
