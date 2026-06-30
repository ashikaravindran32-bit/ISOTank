using System.Text.Json;
using ISOTankApp.Models;

namespace ISOTankApp.Views;

public partial class UploadPhotosPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public bool IsSaved { get; set; } = false;
    public bool IsEditMode { get; set; } = false;

    public UploadPhotosPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient { BaseAddress = new Uri("https://uat.spairyx.com/itankapi/") };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPhotoCategories();
    }

    private async Task LoadPhotoCategories()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/masterdata");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var masterData = JsonSerializer.Deserialize<MasterDataResponse>(content, options);

                if (masterData != null)
                {
                    if (SessionManager.UploadedPhotos.Count == 0)
                    {
                        var photoUploadItems = masterData.ImageTypes.Select(img => new PhotoUploadItem
                        {
                            Category = img,
                            ImagePath = string.Empty
                        }).ToList();
                        
                        SessionManager.UploadedPhotos = photoUploadItems;
                    }
                    
                    BindableLayout.SetItemsSource(PhotosFlexLayout, SessionManager.UploadedPhotos);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load photo categories: {ex.Message}", "OK");
        }
    }

    private async void OnPhotoTapped(object sender, EventArgs e)
    {
        if (e is TappedEventArgs args && args.Parameter is PhotoUploadItem photoItem)
        {
            string action = await DisplayActionSheet($"Upload {photoItem.Category.Name}", "Cancel", null, "Take Photo", "Choose from Gallery");
            
            try
            {
                FileResult photo = null;

                if (action == "Take Photo")
                {
                    if (MediaPicker.Default.IsCaptureSupported)
                    {
                        photo = await MediaPicker.Default.CapturePhotoAsync();
                    }
                    else
                    {
                        await DisplayAlert("Not Supported", "Your device does not currently support taking photos.", "OK");
                        return;
                    }
                }
                else if (action == "Choose from Gallery")
                {
                    photo = await MediaPicker.Default.PickPhotoAsync();
                }

                if (photo != null)
                {
                    // Update the model to display the image preview
                    photoItem.ImagePath = photo.FullPath;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while picking the photo: {ex.Message}", "OK");
            }
        }
    }

    private void OnMarkPhotoTapped(object sender, EventArgs e)
    {
        if (e is TappedEventArgs args && args.Parameter is PhotoUploadItem photoItem)
        {
            if (photoItem.HasImage)
            {
                photoItem.IsMarked = !photoItem.IsMarked;
            }
        }
    }

    private async void OnSavePhotosClicked(object sender, EventArgs e)
    {
        GlobalState.IsPhotosSaved = true;
        await DisplayAlert("Success", "Photos saved successfully. You can now access the Checklist.", "OK");
        await Navigation.PushAsync(new ChecklistPage());
    }

    private async void OnTankInfoTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnChecklistTapped(object sender, EventArgs e)
    {
        if (!GlobalState.IsPhotosSaved && !IsEditMode)
        {
            await DisplayAlert("Locked", "Please save Upload Photos first.", "OK");
            return;
        }
        await Navigation.PushAsync(new ChecklistPage());
    }

    private async void OnReviewTapped(object sender, EventArgs e)
    {
        if (!GlobalState.IsChecklistSaved && !IsEditMode)
        {
            await DisplayAlert("Locked", "Please save Checklist first.", "OK");
            return;
        }
        await Navigation.PushAsync(new ReviewSubmitPage());
    }
}
