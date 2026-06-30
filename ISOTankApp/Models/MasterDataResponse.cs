namespace ISOTankApp.Models;

public class MasterDataResponse
{
    public List<MasterItem> TankStatuses { get; set; } = new();
    public List<MasterItem> InspectionTypes { get; set; } = new();
    public List<MasterItem> Locations { get; set; } = new();
    public List<MasterItem> SafetyValveBrands { get; set; } = new();
    public List<MasterItem> ImageTypes { get; set; } = new();
    public List<TankItem> Tanks { get; set; } = new();
}

public class MasterItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key => Name.ToUpper().Replace(" ", "_");

    public override string ToString() => Name;
}

public class TankItem : MasterItem
{
    public float? CapacityL { get; set; }
    public string Standard { get; set; } = string.Empty;
    public string WorkingPressure { get; set; } = string.Empty;
    public float? GrossKg { get; set; }
    public float? TareWeightKg { get; set; }
    
    public string MfgrName { get; set; } = string.Empty;
    public string Ownership { get; set; } = string.Empty;
    public string Temp { get; set; } = string.Empty;
    public string Frame { get; set; } = string.Empty;
    public string Cabinet { get; set; } = string.Empty;
    public string SVBrand { get; set; } = string.Empty;
    public string NextInspectionDue { get; set; } = "NOT SET";
}

public class PhotoUploadItem : System.ComponentModel.INotifyPropertyChanged
{
    public MasterItem Category { get; set; } = new();
    
    private string _imagePath = string.Empty;
    public string ImagePath
    {
        get => _imagePath;
        set 
        { 
            _imagePath = value; 
            OnPropertyChanged(); 
            OnPropertyChanged(nameof(HasImage)); 
            OnPropertyChanged(nameof(ShowPlaceholder)); 
        }
    }
    
    public bool HasImage => !string.IsNullOrEmpty(ImagePath);
    public bool ShowPlaceholder => string.IsNullOrEmpty(ImagePath);

    private bool _isMarked;
    public bool IsMarked
    {
        get => _isMarked;
        set
        {
            _isMarked = value;
            OnPropertyChanged();
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
