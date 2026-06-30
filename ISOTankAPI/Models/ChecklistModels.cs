namespace ISOTankAPI.Models
{
    public class ChecklistGroup
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public List<ChecklistItem> Items { get; set; } = new();
    }

    public class ChecklistItem
    {
        public int SubJobId { get; set; }
        public string Sn { get; set; }
        public string SubJobName { get; set; }
    }
}
