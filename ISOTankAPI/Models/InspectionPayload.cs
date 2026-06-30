namespace ISOTankAPI.Models
{
    public class InspectionPayload
    {
        public InspectionBasicInfo BasicInfo { get; set; } = new();
        public List<ChecklistPayloadItem> ChecklistItems { get; set; } = new();
        public List<PhotoPayloadItem> Photos { get; set; } = new();
    }

    public class ChecklistPayloadItem
    {
        public int SubJobId { get; set; }
        public bool IsFaulty { get; set; }
        public string Comment { get; set; } = string.Empty;
        public List<string> AssignedPhotoKeys { get; set; } = new();
    }

    public class PhotoPayloadItem
    {
        public string CategoryKey { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public bool IsMarked { get; set; }
    }
}
