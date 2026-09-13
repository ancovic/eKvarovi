using System.Security.Claims;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/faultreports/{faultReportId:int}/media")]
public class FaultMediaController : ControllerBase
{
    private const long MaxFileSize =
        10 * 1024 * 1024;

    private static readonly Dictionary<string, string>
        AllowedImageContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["image/jpeg"] = ".jpg",
                ["image/png"] = ".png",
                ["image/webp"] = ".webp"
            };

    private static readonly Dictionary<string, string>
        AllowedDocumentContentTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["application/pdf"] = ".pdf"
            };

    private readonly EKvaroviDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public FaultMediaController(
        EKvaroviDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<FaultMediaDto>>> GetMedia(
        int faultReportId)
    {
        var report = await _context.FaultReports
            .Include(report => report.Assignments)
            .FirstOrDefaultAsync(report =>
                report.Id == faultReportId);

        if (report is null)
        {
            return NotFound(
                "Prijava kvara nije pronađena.");
        }

        if (!CanViewReport(report))
        {
            return Forbid();
        }

        var media = await _context.FaultMedia
            .Where(item =>
                item.FaultReportId == faultReportId)
            .OrderBy(item => item.UploadedAt)
            .ThenBy(item => item.Id)
            .ToListAsync();

        return Ok(
            media
                .Select(ToDto)
                .ToList());
    }

    [HttpPost("before-photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [Authorize(
        Policy = AuthorizationPolicies.ReporterOnly)]
    public async Task<ActionResult<FaultMediaDto>> UploadBeforePhoto(
        int faultReportId,
        IFormFile file)
    {
        var employeeIdValue =
            User.FindFirstValue(
                AppClaimTypes.EmployeeId);

        if (!int.TryParse(
                employeeIdValue,
                out var employeeId))
        {
            return Forbid();
        }

        var report = await _context.FaultReports
            .Include(report =>
                report.FaultStatus)
            .FirstOrDefaultAsync(report =>
                report.Id == faultReportId);

        if (report is null)
        {
            return NotFound(
                "Prijava kvara nije pronađena.");
        }

        if (report.ReporterId != employeeId)
        {
            return Forbid();
        }

        if (report.IsArchived)
        {
            return BadRequest(
                "Arhiviranoj prijavi nije moguće dodavati privitke.");
        }

        if (report.FaultStatus?.Name != "Zaprimljeno")
        {
            return BadRequest(
                "Početnu fotografiju moguće je dodati samo dok je prijava u statusu Zaprimljeno.");
        }

        if (file is null ||
            file.Length == 0)
        {
            return BadRequest(
                "Odaberi fotografiju.");
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(
                "Fotografija smije imati najviše 10 MB.");
        }

        if (!AllowedImageContentTypes.TryGetValue(
                file.ContentType,
                out var extension))
        {
            return BadRequest(
                "Dopušteni su JPG, PNG i WEBP formati.");
        }

        return await SaveMedia(
            faultReportId,
            null,
            file,
            AttachmentPurpose.BeforePhoto,
            extension);
    }

    [HttpPost("interventions/{interventionId:int}/after-photo")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [Authorize(Roles = "Admin,Technician")]
    public async Task<ActionResult<FaultMediaDto>> UploadAfterPhoto(
        int faultReportId,
        int interventionId,
        IFormFile file)
    {
        var validation =
            await ValidateInterventionUpload(
                faultReportId,
                interventionId);

        if (validation.ErrorResult is not null)
        {
            return validation.ErrorResult;
        }

        if (file is null ||
            file.Length == 0)
        {
            return BadRequest(
                "Odaberi fotografiju.");
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(
                "Fotografija smije imati najviše 10 MB.");
        }

        if (!AllowedImageContentTypes.TryGetValue(
                file.ContentType,
                out var extension))
        {
            return BadRequest(
                "Dopušteni su JPG, PNG i WEBP formati.");
        }

        return await SaveMedia(
            faultReportId,
            interventionId,
            file,
            AttachmentPurpose.AfterPhoto,
            extension);
    }

    [HttpPost("interventions/{interventionId:int}/document")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [Authorize(Roles = "Admin,Technician")]
    public async Task<ActionResult<FaultMediaDto>> UploadDocument(
        int faultReportId,
        int interventionId,
        IFormFile file)
    {
        var validation =
            await ValidateInterventionUpload(
                faultReportId,
                interventionId);

        if (validation.ErrorResult is not null)
        {
            return validation.ErrorResult;
        }

        if (file is null ||
            file.Length == 0)
        {
            return BadRequest(
                "Odaberi dokument.");
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(
                "Dokument smije imati najviše 10 MB.");
        }

        if (!AllowedDocumentContentTypes.TryGetValue(
                file.ContentType,
                out var extension))
        {
            return BadRequest(
                "Dopušten je samo PDF dokument.");
        }

        return await SaveMedia(
            faultReportId,
            interventionId,
            file,
            AttachmentPurpose.Document,
            extension);
    }

    [HttpDelete("{mediaId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(
        int faultReportId,
        int mediaId)
    {
        var media = await _context.FaultMedia
            .Include(item =>
                item.FaultReport)
            .ThenInclude(report =>
                report!.FaultStatus)
            .Include(item =>
                item.Intervention)
            .ThenInclude(intervention =>
                intervention!.WorkAssignment)
            .FirstOrDefaultAsync(item =>
                item.Id == mediaId &&
                item.FaultReportId == faultReportId);

        if (media is null)
        {
            return NotFound();
        }

        var canDelete =
            User.IsInRole("Admin");

        if (!canDelete &&
            User.IsInRole("Reporter") &&
            media.AttachmentPurpose ==
                AttachmentPurpose.BeforePhoto)
        {
            var employeeIdValue =
                User.FindFirstValue(
                    AppClaimTypes.EmployeeId);

            if (int.TryParse(
                    employeeIdValue,
                    out var employeeId))
            {
                canDelete =
                    media.FaultReport is not null &&
                    media.FaultReport.ReporterId ==
                        employeeId &&
                    media.FaultReport.FaultStatus?.Name ==
                        "Zaprimljeno";
            }
        }

        if (!canDelete &&
            User.IsInRole("Technician") &&
            media.Intervention is not null &&
            media.Intervention.WorkAssignment is not null &&
            (media.AttachmentPurpose ==
                AttachmentPurpose.AfterPhoto ||
             media.AttachmentPurpose ==
                AttachmentPurpose.Document))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (int.TryParse(
                    technicianIdValue,
                    out var technicianId))
            {
                canDelete =
                    media.Intervention.WorkAssignment.TechnicianId ==
                        technicianId &&
                    media.Intervention.WorkAssignment.IsActive &&
                    media.Intervention.FinishedAt is null &&
                    media.FaultReport?.IsArchived == false;
            }
        }

        if (!canDelete)
        {
            return Forbid();
        }

        _context.FaultMedia.Remove(media);

        await _context.SaveChangesAsync();

        var physicalPath =
            GetPhysicalPath(
                media.StoredFileName);

        if (System.IO.File.Exists(
                physicalPath))
        {
            System.IO.File.Delete(
                physicalPath);
        }

        return NoContent();
    }

    private async Task<(
        Intervention? Intervention,
        ActionResult<FaultMediaDto>? ErrorResult)>
        ValidateInterventionUpload(
            int faultReportId,
            int interventionId)
    {
        var intervention =
            await _context.Interventions
                .Include(item =>
                    item.WorkAssignment)
                .ThenInclude(assignment =>
                    assignment!.FaultReport)
                .FirstOrDefaultAsync(item =>
                    item.Id == interventionId &&
                    item.WorkAssignment != null &&
                    item.WorkAssignment.FaultReportId ==
                        faultReportId);

        if (intervention is null)
        {
            return (
                null,
                NotFound(
                    "Intervencija nije pronađena."));
        }

        var assignment =
            intervention.WorkAssignment!;

        var report =
            assignment.FaultReport;

        if (report is null)
        {
            return (
                null,
                NotFound(
                    "Prijava kvara nije pronađena."));
        }

        if (!User.IsInRole("Admin"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (!int.TryParse(
                    technicianIdValue,
                    out var technicianId) ||
                assignment.TechnicianId !=
                    technicianId)
            {
                return (
                    null,
                    Forbid());
            }
        }

        if (report.IsArchived)
        {
            return (
                null,
                BadRequest(
                    "Arhiviranoj prijavi nije moguće dodavati privitke."));
        }

        if (!assignment.IsActive)
        {
            return (
                null,
                BadRequest(
                    "Privitke je moguće dodavati samo na aktivnom radnom nalogu."));
        }

        if (intervention.StartedAt is null ||
            intervention.FinishedAt.HasValue)
        {
            return (
                null,
                BadRequest(
                    "Privitke je moguće dodavati samo na intervenciju koja je u tijeku."));
        }

        return (
            intervention,
            null);
    }

    private async Task<ActionResult<FaultMediaDto>> SaveMedia(
        int faultReportId,
        int? interventionId,
        IFormFile file,
        AttachmentPurpose purpose,
        string extension)
    {
        var uploadDirectory =
            GetUploadDirectory();

        Directory.CreateDirectory(
            uploadDirectory);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var physicalPath =
            Path.Combine(
                uploadDirectory,
                storedFileName);

        await using (var stream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write))
        {
            await file.CopyToAsync(
                stream);
        }

        var media =
            new FaultMedia
            {
                FaultReportId =
                    faultReportId,

                InterventionId =
                    interventionId,

                OriginalFileName =
                    Path.GetFileName(
                        file.FileName),

                StoredFileName =
                    storedFileName,

                ContentType =
                    file.ContentType,

                FileSize =
                    file.Length,

                AttachmentPurpose =
                    purpose,

                UploadedAt =
                    DateTime.UtcNow
            };

        try
        {
            _context.FaultMedia.Add(
                media);

            await _context.SaveChangesAsync();
        }
        catch
        {
            if (System.IO.File.Exists(
                    physicalPath))
            {
                System.IO.File.Delete(
                    physicalPath);
            }

            throw;
        }

        return Ok(
            ToDto(media));
    }

    private string GetUploadDirectory()
    {
        var webRoot =
            _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        return Path.Combine(
            webRoot,
            "uploads",
            "faultreports");
    }

    private string GetPhysicalPath(
        string storedFileName)
    {
        return Path.Combine(
            GetUploadDirectory(),
            Path.GetFileName(
                storedFileName));
    }

    private bool CanViewReport(
        FaultReport report)
    {
        if (User.IsInRole("Admin") ||
            User.IsInRole("Manager"))
        {
            return true;
        }

        if (User.IsInRole("Reporter"))
        {
            var employeeIdValue =
                User.FindFirstValue(
                    AppClaimTypes.EmployeeId);

            return
                int.TryParse(
                    employeeIdValue,
                    out var employeeId) &&
                report.ReporterId ==
                    employeeId;
        }

        if (User.IsInRole("Technician"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            return
                int.TryParse(
                    technicianIdValue,
                    out var technicianId) &&
                report.Assignments.Any(
                    assignment =>
                        assignment.TechnicianId ==
                        technicianId);
        }

        return false;
    }

    private FaultMediaDto ToDto(
        FaultMedia media)
    {
        var relativeUrl =
            $"/uploads/faultreports/{media.StoredFileName}";

        return new FaultMediaDto
        {
            Id =
                media.Id,

            FaultReportId =
                media.FaultReportId,

            InterventionId =
                media.InterventionId,

            Url =
                $"{Request.Scheme}://{Request.Host}{relativeUrl}",

            OriginalFileName =
                media.OriginalFileName,

            ContentType =
                media.ContentType,

            FileSize =
                media.FileSize,

            Purpose =
                media.AttachmentPurpose.ToString(),

            UploadedAt =
                media.UploadedAt
        };
    }
}