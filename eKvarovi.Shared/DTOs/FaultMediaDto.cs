namespace eKvarovi.Shared.DTOs;

public class FaultMediaDto
{
    public int Id { get; set; }

    public int FaultReportId { get; set; }
    public int? InterventionId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }
}