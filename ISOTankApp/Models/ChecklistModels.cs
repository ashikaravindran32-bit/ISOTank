using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ISOTankApp.Models;

public class ChecklistGroup : INotifyPropertyChanged
{
    public int JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public ObservableCollection<ChecklistItem> Items { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ChecklistItem : INotifyPropertyChanged
{
    public int SubJobId { get; set; }
    public string Sn { get; set; } = string.Empty;
    public string SubJobName { get; set; } = string.Empty;
    public int JobId { get; set; }

    // UI state properties
    private bool _isFaulty = false;
    public bool IsFaulty
    {
        get => _isFaulty;
        set
        {
            _isFaulty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusIcon));
            OnPropertyChanged(nameof(ShowAssignmentOptions));
        }
    }

    private string _comment = string.Empty;
    public string Comment
    {
        get => _comment;
        set
        {
            _comment = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasComment));
            OnPropertyChanged(nameof(StatusColor));
        }
    }

    public bool HasComment => !string.IsNullOrWhiteSpace(Comment);

    public Color StatusColor => IsFaulty ? Color.FromArgb("#DC3545") : (HasComment ? Color.FromArgb("#FFC107") : Color.FromArgb("#28A745"));
    public string StatusIcon => IsFaulty ? "\uf111" : "\uf058"; // FontAwesome circle vs check-circle
    
    public string DisplayTitle => $"{Sn} {SubJobName}";

    public ObservableCollection<string> AssignedPhotoKeys { get; set; } = new();

    public IEnumerable<PhotoUploadItem> AssignedPhotos => 
        SessionManager.UploadedPhotos.Where(p => AssignedPhotoKeys.Contains(p.Category.Key) && p.HasImage);

    private ObservableCollection<PhotoAssignmentOption> _assignmentOptions = new();
    public ObservableCollection<PhotoAssignmentOption> AssignmentOptions
    {
        get => _assignmentOptions;
        set
        {
            _assignmentOptions = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasAssignmentOptions));
        }
    }
    
    public bool HasAssignmentOptions => AssignmentOptions.Count > 0;
    public bool ShowAssignmentOptions => IsFaulty && HasAssignmentOptions;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PhotoAssignmentOption : INotifyPropertyChanged
{
    public PhotoUploadItem Photo { get; set; } = new();
    public ChecklistItem ParentItem { get; set; } = null!;
    
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
            
            if (ParentItem != null && Photo != null)
            {
                if (value && !ParentItem.AssignedPhotoKeys.Contains(Photo.Category.Key))
                {
                    ParentItem.AssignedPhotoKeys.Add(Photo.Category.Key);
                }
                else if (!value && ParentItem.AssignedPhotoKeys.Contains(Photo.Category.Key))
                {
                    ParentItem.AssignedPhotoKeys.Remove(Photo.Category.Key);
                }
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
