using System.Text;
using System.Text.Json;

namespace ISOTankApp;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public MainPage()
    {
        InitializeComponent();
        
        // Note: For Android Emulator, use 10.0.2.2 instead of localhost
        // For iOS Simulator, use localhost
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://uat.spairyx.com/itankapi/")
        };
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        LoadingSpinner.IsRunning = true;
        LoadingSpinner.IsVisible = true;

        var inspectionData = new
        {
            ReportNumber = ReportNumberEntry.Text ?? "",
            TankNumber = TankNumberEntry.Text ?? "",
            FrameType = FrameTypePicker.SelectedItem?.ToString() ?? "",
            CabinetType = CabinetTypePicker.SelectedItem?.ToString() ?? "",
            Manufacturer = ManufacturerEntry.Text ?? "",
            VacuumReading = VacuumReadingEntry.Text ?? "",
            Notes = NotesEditor.Text ?? ""
        };

        try
        {
            string json = JsonSerializer.Serialize(inspectionData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/inspection/basic-info", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Success", "Inspection saved successfully!", "OK");
                // Clear fields
                ReportNumberEntry.Text = string.Empty;
                TankNumberEntry.Text = string.Empty;
                ManufacturerEntry.Text = string.Empty;
                VacuumReadingEntry.Text = string.Empty;
                NotesEditor.Text = string.Empty;
                FrameTypePicker.SelectedIndex = -1;
                CabinetTypePicker.SelectedIndex = -1;
            }
            else
            {
                await DisplayAlert("Error", $"Failed to save. Status code: {response.StatusCode}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Network error: {ex.Message}", "OK");
        }
        finally
        {
            LoadingSpinner.IsRunning = false;
            LoadingSpinner.IsVisible = false;
        }
    }
}
