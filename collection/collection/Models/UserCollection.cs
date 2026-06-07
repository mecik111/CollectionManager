namespace collection.Models;

public class UserCollection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<CollectionItem> Items { get; set; } = new();
}
