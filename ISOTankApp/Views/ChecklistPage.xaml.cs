using System.Text.Json;
using ISOTankApp.Models;

namespace ISOTankApp.Views;

public partial class ChecklistPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private List<ChecklistGroup> _checklistGroups = new();

    public ChecklistPage()
    {
        InitializeComponent();
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://uat.spairyx.com/itankapi/")
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_checklistGroups.Count == 0)
        {
            await LoadChecklist();
        }
    }

    private async Task LoadChecklist()
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            var response = await _httpClient.GetAsync("api/checklist/template");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                if (SessionManager.ChecklistState.Count == 0)
                {
                    SessionManager.ChecklistState = JsonSerializer.Deserialize<List<ChecklistGroup>>(content, options);
                }

                if (SessionManager.ChecklistState != null)
                {
                    _checklistGroups = SessionManager.ChecklistState;

                    var markedPhotos = new System.Collections.ObjectModel.ObservableCollection<PhotoUploadItem>(
                        SessionManager.UploadedPhotos.Where(p => p.IsMarked && p.HasImage)
                    );

                    foreach (var group in _checklistGroups)
                    {
                        foreach (var item in group.Items)
                        {
                            item.AssignmentOptions = new System.Collections.ObjectModel.ObservableCollection<PhotoAssignmentOption>(
                                markedPhotos.Select(p => new PhotoAssignmentOption 
                                { 
                                    Photo = p, 
                                    ParentItem = item,
                                    IsSelected = item.AssignedPhotoKeys.Contains(p.Category.Key)
                                })
                            );
                        }
                    }

                    ChecklistCollectionView.ItemsSource = _checklistGroups;
                }
            }
            else
            {
                await DisplayAlert("Error", $"Failed to load checklist: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Network error: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is ChecklistItem item)
        {
            item.IsFaulty = !item.IsFaulty; // Toggle red/green
        }
    }

    private void OnAssignPhotoTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is PhotoAssignmentOption option)
        {
            option.IsSelected = !option.IsSelected;
        }
    }

    private async void OnSaveAndNextTapped(object sender, EventArgs e)
    {
        // Find all faulty items
        var faultyItems = _checklistGroups.SelectMany(g => g.Items).Where(i => i.IsFaulty).ToList();

        if (faultyItems.Any())
        {
            // Pass the faulty items to the Resolution Required Page
            await Navigation.PushAsync(new ResolutionRequiredPage(faultyItems, this));
        }
        else
        {
            // No faulty items, move directly to Review & Submit
            GlobalState.IsChecklistSaved = true;
            await DisplayAlert("Success", "Checklist saved successfully.", "OK");
            await Navigation.PushAsync(new ReviewSubmitPage());
        }
    }

    // Navigation callbacks
    private async void OnTankInfoTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnUploadPhotosTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnReviewTapped(object sender, EventArgs e)
    {
        if (!GlobalState.IsChecklistSaved)
        {
            await DisplayAlert("Locked", "Please save Checklist first.", "OK");
            return;
        }
        await Navigation.PushAsync(new ReviewSubmitPage());
    }
}
