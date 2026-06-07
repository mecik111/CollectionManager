using System.Collections.ObjectModel;
using collection.Models;
using collection.Services;

namespace collection.Views;

public partial class MainPage : ContentPage
{
    private readonly CollectionStorageService _storageService = new();
    private readonly ObservableCollection<UserCollection> _collections = new();

    private static readonly List<string> Categories =
    [
        "Książki",
        "Gry na konsole",
        "Gry planszowe",
        "Zestawy LEGO",
        "Karty TCG",
        "Płyty muzyczne",
        "Inne"
    ];

    public MainPage()
    {
        InitializeComponent();
        CategoryPicker.ItemsSource = Categories;
        CategoryPicker.SelectedIndex = 0;
        CollectionsView.ItemsSource = _collections;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if DEBUG
        System.Diagnostics.Debug.WriteLine($"Plik danych: {_storageService.DataFilePath}");
#endif

        if (_collections.Count > 0)
        {
            return;
        }

        var loadedCollections = await _storageService.LoadAsync();
        foreach (var collection in loadedCollections)
        {
            _collections.Add(collection);
        }
    }

    private async void OnAddCollectionClicked(object? sender, EventArgs e)
    {
        var name = CollectionNameEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Błąd", "Podaj nazwę kolekcji.", "OK");
            return;
        }

        var category = CategoryPicker.SelectedItem?.ToString() ?? "Inne";

        _collections.Add(new UserCollection
        {
            Name = name,
            Category = category
        });

        await _storageService.SaveAsync(_collections);
        CollectionNameEntry.Text = string.Empty;
        CollectionNameEntry.Focus();
    }

    private async void OnOpenCollectionClicked(object? sender, EventArgs e)
    {
        if (CollectionsView.SelectedItem is not UserCollection selected)
        {
            await DisplayAlert("Informacja", "Zaznacz kolekcję do otwarcia.", "OK");
            return;
        }

        await Navigation.PushAsync(new CollectionDetailsPage(selected, SaveAllAsync));
    }

    private Task SaveAllAsync()
    {
        return _storageService.SaveAsync(_collections);
    }
}
