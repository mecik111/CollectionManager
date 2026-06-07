using System.Collections.ObjectModel;
using collection.Models;

namespace collection.Views;

public partial class CollectionDetailsPage : ContentPage
{
    private readonly UserCollection _collection;
    private readonly ObservableCollection<CollectionItem> _items;
    private readonly Func<Task> _saveAction;

    public CollectionDetailsPage(UserCollection collection, Func<Task> saveAction)
    {
        InitializeComponent();
        _collection = collection;
        _saveAction = saveAction;
        _items = new ObservableCollection<CollectionItem>(_collection.Items);
        ItemsView.ItemsSource = _items;
        Title = _collection.Name;
        TitleLabel.Text = $"{_collection.Name} ({_collection.Category})";
    }

    private async void OnAddItemClicked(object? sender, EventArgs e)
    {
        var editorPage = new ItemEditorPage();
        var result = await editorPage.GetResultAsync(this);
        if (result is null)
        {
            return;
        }

        _items.Add(result);
        await SaveAsync();
    }

    private async void OnEditItemClicked(object? sender, EventArgs e)
    {
        if (ItemsView.SelectedItem is not CollectionItem selected)
        {
            await DisplayAlert("Informacja", "Zaznacz element do edycji.", "OK");
            return;
        }

        var editorPage = new ItemEditorPage(selected);
        var result = await editorPage.GetResultAsync(this);
        if (result is null)
        {
            return;
        }

        var index = _items.IndexOf(selected);
        if (index < 0)
        {
            return;
        }

        result.Id = selected.Id;
        _items[index] = result;
        ItemsView.SelectedItem = result;
        await SaveAsync();
    }

    private async void OnDeleteItemClicked(object? sender, EventArgs e)
    {
        if (ItemsView.SelectedItem is not CollectionItem selected)
        {
            await DisplayAlert("Informacja", "Zaznacz element do usunięcia.", "OK");
            return;
        }

        var shouldDelete = await DisplayAlert("Potwierdzenie", $"Usunąć '{selected.Name}'?", "Tak", "Nie");
        if (!shouldDelete)
        {
            return;
        }

        _items.Remove(selected);
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        _collection.Items = _items.ToList();
        await _saveAction();
    }
}
