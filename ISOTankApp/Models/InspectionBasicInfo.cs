namespace ISOTankApp.Models;

public class InspectionBasicInfo
{
    public int Id { get; set; }
    public int InspectionId { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public string TankNumber { get; set; } = string.Empty;
    public string FrameType { get; set; } = string.Empty;
    public string CabinetType { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string VacuumReading { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public int? StatusId { get; set; }
    public int? InspectionTypeId { get; set; }
    public int? LocationId { get; set; }
    public int? SafetyValveBrandId { get; set; }
}
