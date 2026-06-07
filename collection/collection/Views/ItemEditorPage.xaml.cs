using System.Globalization;
using collection.Models;

namespace collection.Views;

public partial class ItemEditorPage : ContentPage
{
    private TaskCompletionSource<CollectionItem?>? _resultSource;

    public ItemEditorPage(CollectionItem? item = null)
    {
        InitializeComponent();

        StatusPicker.ItemsSource = new List<string>
        {
            "Nowy",
            "Użyty",
            "Na sprzedaż",
            "Sprzedany",
            "Chcę kupić"
        };

        if (item is null)
        {
            Title = "Dodaj element";
            StatusPicker.SelectedIndex = 0;
            RatingStepper.Value = 1;
            return;
        }

        Title = "Edycja elementu";
        NameEntry.Text = item.Name;
        PriceEntry.Text = item.Price.ToString(CultureInfo.InvariantCulture);
        StatusPicker.SelectedItem = item.Status;
        if (StatusPicker.SelectedIndex < 0)
        {
            StatusPicker.SelectedIndex = 0;
        }

        RatingStepper.Value = Math.Clamp(item.Rating, 1, 10);
        RatingLabel.Text = $"{(int)RatingStepper.Value}/10";
    }

    public async Task<CollectionItem?> GetResultAsync(Page owner)
    {
        _resultSource = new TaskCompletionSource<CollectionItem?>();
        await owner.Navigation.PushModalAsync(new NavigationPage(this));
        return await _resultSource.Task;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Błąd", "Nazwa elementu jest wymagana.", "OK");
            return;
        }

        if (!decimal.TryParse(PriceEntry.Text?.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
        {
            price = 0;
        }

        var result = new CollectionItem
        {
            Name = name,
            Price = price,
            Status = StatusPicker.SelectedItem?.ToString() ?? "Nowy",
            Rating = (int)RatingStepper.Value
        };

        _resultSource?.SetResult(result);
        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        _resultSource?.SetResult(null);
        await Navigation.PopModalAsync();
    }

    private void OnRatingChanged(object? sender, ValueChangedEventArgs e)
    {
        var newValue = Math.Round(e.NewValue);
        if (RatingStepper.Value != newValue)
        {
            RatingStepper.Value = newValue;
        }
        RatingLabel.Text = $"{(int)newValue}/10";
    }
}
