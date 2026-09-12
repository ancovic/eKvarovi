namespace eKvarovi.Shared.DTOs;

public class ReviewFaultReportDto
{
    public int FaultTypeId { get; set; }
    public int FaultPriorityId { get; set; }
    public DateTime? DueDate { get; set; }
}