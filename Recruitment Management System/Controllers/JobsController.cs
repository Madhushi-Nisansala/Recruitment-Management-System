using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment_Management_System.Data;
using Recruitment_Management_System.DTOs.Jobs;
using Recruitment_Management_System.Models;

namespace Recruitment_Management_System.Controllers;

[ApiController]
[Route("api/jobs")]
public sealed class JobsController(ApplicationDbContext dbContext) : ControllerBase
{
    private const int DefaultPageSize = 10;
    private const int MaximumPageSize = 50;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetJobs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaximumPageSize)
        {
            return BadRequest(new
            {
                message = $"pageNumber must be at least 1 and pageSize must be between 1 and {MaximumPageSize}."
            });
        }

        var query = dbContext.Jobs
            .AsNoTracking()
            .Include(job => job.Recruiter)
            .OrderByDescending(job => job.PostedAt);
        var totalCount = await query.CountAsync(cancellationToken);
        var jobs = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(job => new JobResponseDto(
                job.Id,
                job.Title,
                job.Description,
                job.Location,
                job.EmploymentType,
                job.SalaryMin,
                job.SalaryMax,
                job.PostedAt,
                job.ClosingDate,
                new RecruiterSummaryDto(
                    job.Recruiter.Id,
                    job.Recruiter.FullName,
                    job.Recruiter.Email)))
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            pageNumber,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            items = jobs
        });
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .AsNoTracking()
            .Include(candidate => candidate.Recruiter)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return job is null ? NotFound() : Ok(ToResponse(job));
    }

    [HttpPost]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<JobResponseDto>> Create(
        CreateJobDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Title)] = ["Title is required."],
                [nameof(request.Description)] = ["Description is required."]
            }));
        }

        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var recruiterId = currentUserId.Value;
        if (User.IsInRole("Admin"))
        {
            if (!request.RecruiterId.HasValue)
            {
                return BadRequest(new { message = "RecruiterId is required when an Admin creates a job." });
            }

            recruiterId = request.RecruiterId.Value;
        }
        else if (request.RecruiterId.HasValue && request.RecruiterId.Value != currentUserId.Value)
        {
            return Forbid();
        }

        var recruiter = await dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Id == recruiterId && user.Role == "Recruiter",
                cancellationToken);

        if (recruiter is null)
        {
            return BadRequest(new { message = "The selected user is not an existing Recruiter." });
        }

        var job = new Job
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Location = NormalizeOptional(request.Location),
            EmploymentType = NormalizeOptional(request.EmploymentType),
            SalaryMin = request.SalaryMin,
            SalaryMax = request.SalaryMax,
            PostedAt = DateTime.UtcNow,
            ClosingDate = request.ClosingDate,
            RecruiterId = recruiterId,
            Recruiter = recruiter
        };

        if (job.ClosingDate.HasValue && job.ClosingDate <= job.PostedAt)
        {
            return BadRequest(new { message = "ClosingDate must be later than PostedAt." });
        }

        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = job.Id }, ToResponse(job));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<ActionResult<JobResponseDto>> Update(
        int id,
        UpdateJobDto request,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .Include(candidate => candidate.Recruiter)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin") && !OwnsJob(job))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.Title)] = ["Title is required."],
                [nameof(request.Description)] = ["Description is required."]
            }));
        }

        if (request.ClosingDate.HasValue && request.ClosingDate <= job.PostedAt)
        {
            return BadRequest(new { message = "ClosingDate must be later than PostedAt." });
        }

        job.Title = request.Title.Trim();
        job.Description = request.Description.Trim();
        job.Location = NormalizeOptional(request.Location);
        job.EmploymentType = NormalizeOptional(request.EmploymentType);
        job.SalaryMin = request.SalaryMin;
        job.SalaryMax = request.SalaryMax;
        job.ClosingDate = request.ClosingDate;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(job));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Recruiter,Admin")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        if (!User.IsInRole("Admin") && !OwnsJob(job))
        {
            return Forbid();
        }

        var hasApplications = await dbContext.Applications
            .AnyAsync(application => application.JobId == id, cancellationToken);

        if (hasApplications)
        {
            return Conflict(new
            {
                message = "This job cannot be deleted because applications already reference it."
            });
        }

        dbContext.Jobs.Remove(job);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool OwnsJob(Job job) =>
        GetCurrentUserId() == job.RecruiterId;

    private int? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static JobResponseDto ToResponse(Job job) =>
        new(
            job.Id,
            job.Title,
            job.Description,
            job.Location,
            job.EmploymentType,
            job.SalaryMin,
            job.SalaryMax,
            job.PostedAt,
            job.ClosingDate,
            new RecruiterSummaryDto(
                job.Recruiter.Id,
                job.Recruiter.FullName,
                job.Recruiter.Email));
}
