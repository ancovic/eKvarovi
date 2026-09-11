namespace eKvarovi.Shared.Models;

public enum AttachmentPurpose
{
    BeforePhoto = 1,
    AfterPhoto = 2,
    Document = 3
}

public class FaultMedia
{
    public int Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public AttachmentPurpose AttachmentPurpose { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int FaultReportId { get; set; }
    public FaultReport? FaultReport { get; set; }

    public int? InterventionId { get; set; }
    public Intervention? Intervention { get; set; }
}