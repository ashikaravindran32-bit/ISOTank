using ISOTankApp.Models;

namespace ISOTankApp.Views;

public partial class ResolutionRequiredPage : ContentPage
{
    private readonly List<ChecklistItem> _faultyItems;
    private readonly Page _parentPage;

    public ResolutionRequiredPage(List<ChecklistItem> faultyItems, Page parentPage)
    {
        InitializeComponent();
        _faultyItems = faultyItems;
        _parentPage = parentPage;
        
        BindableLayout.SetItemsSource(FaultyItemsLayout, _faultyItems);
    }

    private void OnItemResolvedTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is ChecklistItem item)
        {
            // Tapping toggles it between faulty (red) and resolved (green)
            item.IsFaulty = !item.IsFaulty;
        }
    }

    private async void OnSaveContinueTapped(object sender, EventArgs e)
    {
        // Check if any item is STILL faulty (red)
        if (_faultyItems.Any(i => i.IsFaulty))
        {
            await DisplayAlert("Incomplete", "Please resolve all faulty items before proceeding.", "OK");
            return;
        }

        // All resolved!
        GlobalState.IsChecklistSaved = true;
        await DisplayAlert("Success", "To-do list updated!", "OK");
        
        // Push the review submit page
        await Navigation.PushAsync(new ReviewSubmitPage());
        
        // Remove this Resolution page and Checklist page from stack to keep history clean
        Navigation.RemovePage(this);
        if (_parentPage != null && Navigation.NavigationStack.Contains(_parentPage))
        {
            Navigation.RemovePage(_parentPage);
        }
    }

    private async void OnCancelTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
