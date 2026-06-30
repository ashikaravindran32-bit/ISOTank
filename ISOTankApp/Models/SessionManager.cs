namespace ISOTankApp.Models;

public static class SessionManager
{
    // Tank Info State
    public static int? InspectionId { get; set; } // Added for Edit mode
    public static TankItem? SelectedTank { get; set; }
    public static string? SelectedTankStatus { get; set; }
    public static int? SelectedTankStatusId { get; set; }
    public static string? SelectedInspectionType { get; set; }
    public static int? SelectedInspectionTypeId { get; set; }
    public static string? SelectedLocation { get; set; }
    public static int? SelectedLocationId { get; set; }
    public static string? SelectedSafetyValve { get; set; }
    public static int? SelectedSafetyValveId { get; set; }
    public static string? VacuumReading { get; set; }
    public static string? LifterWeight { get; set; }

    // Upload Photos State
    public static List<PhotoUploadItem> UploadedPhotos { get; set; } = new();

    // Checklist State
    public static List<ChecklistGroup> ChecklistState { get; set; } = new();

    // Clear session when starting a new inspection
    public static void Clear()
    {
        InspectionId = null;
        SelectedTank = null;
        SelectedTankStatus = null;
        SelectedTankStatusId = null;
        SelectedInspectionType = null;
        SelectedInspectionTypeId = null;
        SelectedLocation = null;
        SelectedLocationId = null;
        SelectedSafetyValve = null;
        SelectedSafetyValveId = null;
        VacuumReading = null;
        LifterWeight = null;
        UploadedPhotos.Clear();
        ChecklistState.Clear();
    }
}
