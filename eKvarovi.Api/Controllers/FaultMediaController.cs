using System.Security.Claims;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
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
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<List<FaultMediaDto>>> GetMedia(
        int faultReportId)
    {
        var report = await _context.FaultReports
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
    [Microsoft.AspNetCore.Authorization.Authorize(
        Policy = eKvarovi.Api.Security.AuthorizationPolicies.ReporterOnly)]
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

        var webRoot =
            _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        var uploadDirectory = Path.Combine(
            webRoot,
            "uploads",
            "faultreports");

        Directory.CreateDirectory(
            uploadDirectory);

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var physicalPath = Path.Combine(
            uploadDirectory,
            storedFileName);

        await using (var stream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write))
        {
            await file.CopyToAsync(stream);
        }

        var media = new FaultMedia
        {
            FaultReportId = faultReportId,

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
                AttachmentPurpose.BeforePhoto,

            UploadedAt =
                DateTime.UtcNow,

            InterventionId =
                null
        };

        try
        {
            _context.FaultMedia.Add(media);
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

        return Ok(ToDto(media));
    }

    [HttpDelete("{mediaId:int}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Delete(
        int faultReportId,
        int mediaId)
    {
        var media = await _context.FaultMedia
            .Include(item =>
                item.FaultReport)
            .ThenInclude(report =>
                report!.FaultStatus)
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

        if (!canDelete)
        {
            return Forbid();
        }

        _context.FaultMedia.Remove(media);
        await _context.SaveChangesAsync();

        var webRoot =
            _environment.WebRootPath
            ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        var physicalPath = Path.Combine(
            webRoot,
            "uploads",
            "faultreports",
            Path.GetFileName(
                media.StoredFileName));

        if (System.IO.File.Exists(
                physicalPath))
        {
            System.IO.File.Delete(
                physicalPath);
        }

        return NoContent();
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

        return false;
    }

    private FaultMediaDto ToDto(
        FaultMedia media)
    {
        var relativeUrl =
            $"/uploads/faultreports/{media.StoredFileName}";

        return new FaultMediaDto
        {
            Id = media.Id,
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