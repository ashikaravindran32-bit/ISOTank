using System.Text.Json;
using ISOTankApp.Models;

namespace ISOTankApp.Views;

public partial class TankInfoPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public bool IsSaved { get; set; } = false;
    public bool IsEditMode { get; set; } = false;

    public TankInfoPage()
    {
        InitializeComponent();
        
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://uat.spairyx.com/itankapi/")
        };
        
        VacuumUomPicker.ItemsSource = new List<string> { "BAR", "PSI", "mmHg", "Pa", "atm", "kPa" };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMasterData();
        
        if (IsEditMode && SessionManager.InspectionId.HasValue)
        {
            await LoadExistingInspection(SessionManager.InspectionId.Value);
        }
        else if (!SessionManager.InspectionId.HasValue && !GlobalState.IsPhotosSaved && !GlobalState.IsChecklistSaved)
        {
            // Reset form if session was cleared
            TankPicker.SelectedItem = null;
            TankStatusPicker.SelectedItem = null;
            InspectionTypePicker.SelectedItem = null;
            LocationPicker.SelectedItem = null;
            SafetyValvePicker.SelectedItem = null;
            VacuumReadingEntry.Text = string.Empty;
            LifterWeightEntry.Text = string.Empty;
            IsSaved = false;
            
            TabUploadPhotosCircle.BackgroundColor = Color.FromArgb("#E9ECEF");
            TabUploadPhotosText.TextColor = Color.FromArgb("#ADB5BD");
            
            // Master Data Panel reset
            SpecMfgr.Text = "-";
            SpecOwnership.Text = "-";
            SpecPressure.Text = "-";
            SpecTemp.Text = "-";
            SpecFrame.Text = "-";
            SpecCabinet.Text = "-";
            SpecSVBrand.Text = "-";
            SpecNextInspection.Text = "NOT SET";
        }
    }

    private async Task LoadExistingInspection(int id)
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            var response = await _httpClient.GetAsync($"api/inspection/basic-info/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var info = JsonSerializer.Deserialize<InspectionBasicInfo>(content, options);

                if (info != null)
                {
                    // Select Tank
                    if (!string.IsNullOrEmpty(info.TankNumber) && TankPicker.ItemsSource != null)
                    {
                        var tank = TankPicker.ItemsSource.Cast<TankItem>().FirstOrDefault(t => t.Name == info.TankNumber);
                        if (tank != null)
                        {
                            TankPicker.SelectedItem = tank;
                        }
                    }

                    // Select Pickers by ID
                    if (info.StatusId.HasValue && TankStatusPicker.ItemsSource != null)
                        TankStatusPicker.SelectedItem = TankStatusPicker.ItemsSource.Cast<MasterItem>().FirstOrDefault(m => m.Id == info.StatusId.Value);
                        
                    if (info.InspectionTypeId.HasValue && InspectionTypePicker.ItemsSource != null)
                        InspectionTypePicker.SelectedItem = InspectionTypePicker.ItemsSource.Cast<MasterItem>().FirstOrDefault(m => m.Id == info.InspectionTypeId.Value);
                        
                    if (info.LocationId.HasValue && LocationPicker.ItemsSource != null)
                        LocationPicker.SelectedItem = LocationPicker.ItemsSource.Cast<MasterItem>().FirstOrDefault(m => m.Id == info.LocationId.Value);
                        
                    if (info.SafetyValveBrandId.HasValue && SafetyValvePicker.ItemsSource != null)
                        SafetyValvePicker.SelectedItem = SafetyValvePicker.ItemsSource.Cast<MasterItem>().FirstOrDefault(m => m.Id == info.SafetyValveBrandId.Value);

                    VacuumReadingEntry.Text = info.VacuumReading;
                }
            }
            else
            {
                await DisplayAlert("Error", $"Failed to load existing inspection: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Network Error", $"Could not load inspection data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }

    private async Task LoadMasterData()
    {
        try
        {
            LoadingSpinner.IsVisible = true;
            LoadingSpinner.IsRunning = true;

            var response = await _httpClient.GetAsync("api/masterdata");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var masterData = JsonSerializer.Deserialize<MasterDataResponse>(content, options);

                if (masterData != null)
                {
                    TankPicker.ItemsSource = masterData.Tanks;
                    TankStatusPicker.ItemsSource = masterData.TankStatuses;
                    InspectionTypePicker.ItemsSource = masterData.InspectionTypes;
                    LocationPicker.ItemsSource = masterData.Locations;
                    SafetyValvePicker.ItemsSource = masterData.SafetyValveBrands;
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("API Error", $"Failed to fetch data: {response.StatusCode}\n{error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Network Error", $"Could not connect to API: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsVisible = false;
            LoadingSpinner.IsRunning = false;
        }
    }

    private void OnTankSelected(object sender, EventArgs e)
    {
        if (TankPicker.SelectedItem is TankItem selectedTank)
        {
            // Update Master Data panel with actual DB values
            SpecMfgr.Text = string.IsNullOrEmpty(selectedTank.MfgrName) ? "N/A" : selectedTank.MfgrName;
            SpecOwnership.Text = string.IsNullOrEmpty(selectedTank.Ownership) ? "N/A" : selectedTank.Ownership;
            SpecPressure.Text = string.IsNullOrEmpty(selectedTank.WorkingPressure) ? "N/A" : selectedTank.WorkingPressure + " BAR";
            SpecTemp.Text = string.IsNullOrEmpty(selectedTank.Temp) ? "N/A" : selectedTank.Temp;
            SpecFrame.Text = string.IsNullOrEmpty(selectedTank.Frame) ? "N/A" : selectedTank.Frame;
            SpecCabinet.Text = string.IsNullOrEmpty(selectedTank.Cabinet) ? "N/A" : selectedTank.Cabinet;
            SpecSVBrand.Text = string.IsNullOrEmpty(selectedTank.SVBrand) ? "N/A" : selectedTank.SVBrand;
            SpecNextInspection.Text = selectedTank.NextInspectionDue;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (TankPicker.SelectedItem == null)
        {
            await DisplayAlert("Validation", "Please select a tank.", "OK");
            return;
        }

        // Save to Session Manager
        SessionManager.SelectedTank = TankPicker.SelectedItem as TankItem;
        SessionManager.SelectedTankStatus = TankStatusPicker.SelectedItem?.ToString() ?? "";
        SessionManager.SelectedTankStatusId = (TankStatusPicker.SelectedItem as MasterItem)?.Id;
        SessionManager.SelectedInspectionType = InspectionTypePicker.SelectedItem?.ToString() ?? "";
        SessionManager.SelectedInspectionTypeId = (InspectionTypePicker.SelectedItem as MasterItem)?.Id;
        SessionManager.SelectedLocation = LocationPicker.SelectedItem?.ToString() ?? "";
        SessionManager.SelectedLocationId = (LocationPicker.SelectedItem as MasterItem)?.Id;
        SessionManager.SelectedSafetyValve = SafetyValvePicker.SelectedItem?.ToString() ?? "";
        SessionManager.SelectedSafetyValveId = (SafetyValvePicker.SelectedItem as MasterItem)?.Id;
        var uom = VacuumUomPicker.SelectedItem?.ToString() ?? "";
        var reading = VacuumReadingEntry.Text ?? "N/A";
        SessionManager.VacuumReading = string.IsNullOrWhiteSpace(reading) || reading == "N/A" 
            ? "N/A" 
            : string.IsNullOrWhiteSpace(uom) ? reading : $"{reading} {uom}";

        SessionManager.LifterWeight = LifterWeightEntry.Text ?? "0";

        IsSaved = true;
        GlobalState.IsTankInfoSaved = true;
        
        // Update UI of ONLY Upload Photos to show it is unlocked
        TabUploadPhotosCircle.BackgroundColor = Color.FromArgb("#28A745");
        TabUploadPhotosText.TextColor = Color.FromArgb("#28A745");
        
        await Navigation.PushAsync(new UploadPhotosPage { IsEditMode = this.IsEditMode, IsSaved = this.IsSaved });
    }

    private async void OnUploadPhotosTapped(object sender, EventArgs e)
    {
        if (!GlobalState.IsTankInfoSaved && !IsEditMode)
        {
            await DisplayAlert("Locked", "Please save the Tank Info first.", "OK");
            return;
        }
        await Navigation.PushAsync(new UploadPhotosPage { IsEditMode = this.IsEditMode, IsSaved = this.IsSaved });
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

public static class GlobalState
{
    public static bool IsTankInfoSaved { get; set; } = false;
    public static bool IsPhotosSaved { get; set; } = false;
    public static bool IsChecklistSaved { get; set; } = false;
}
