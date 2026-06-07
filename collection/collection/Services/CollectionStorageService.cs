using System.Globalization;
using System.Text;
using collection.Models;

namespace collection.Services;

public class CollectionStorageService
{
    private const char Separator = '|';
    private readonly string _filePath;

    public CollectionStorageService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "collections.txt");
    }

    public string DataFilePath => _filePath;

    public async Task<List<UserCollection>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<UserCollection>();
        }

        var collections = new Dictionary<Guid, UserCollection>();
        var lines = await File.ReadAllLinesAsync(_filePath);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = SplitEscaped(line);
            if (parts.Count == 0)
            {
                continue;
            }

            if (parts[0] == "COLLECTION" && parts.Count >= 4 && Guid.TryParse(parts[1], out var collectionId))
            {
                collections[collectionId] = new UserCollection
                {
                    Id = collectionId,
                    Name = Unescape(parts[2]),
                    Category = Unescape(parts[3])
                };
                continue;
            }

            if (parts[0] == "ITEM" && parts.Count >= 7 && Guid.TryParse(parts[1], out var ownerId) && Guid.TryParse(parts[2], out var itemId))
            {
                if (!collections.TryGetValue(ownerId, out var collection))
                {
                    continue;
                }

                decimal.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
                int.TryParse(parts[6], out var rating);

                collection.Items.Add(new CollectionItem
                {
                    Id = itemId,
                    Name = Unescape(parts[3]),
                    Price = price,
                    Status = Unescape(parts[5]),
                    Rating = Math.Clamp(rating, 1, 10)
                });
            }
        }

        return collections.Values.OrderBy(x => x.Name).ToList();
    }

    public async Task SaveAsync(IEnumerable<UserCollection> collections)
    {
        var lines = new List<string>();

        foreach (var collection in collections)
        {
            lines.Add($"COLLECTION{Separator}{collection.Id}{Separator}{Escape(collection.Name)}{Separator}{Escape(collection.Category)}");

            foreach (var item in collection.Items)
            {
                lines.Add(
                    $"ITEM{Separator}{collection.Id}{Separator}{item.Id}{Separator}{Escape(item.Name)}{Separator}{item.Price.ToString(CultureInfo.InvariantCulture)}{Separator}{Escape(item.Status)}{Separator}{Math.Clamp(item.Rating, 1, 10)}");
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        await File.WriteAllLinesAsync(_filePath, lines);
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("|", "\\|");
    }

    private static string Unescape(string value)
    {
        var output = new StringBuilder();
        var isEscaped = false;

        foreach (var ch in value)
        {
            if (isEscaped)
            {
                output.Append(ch switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    _ => ch
                });
                isEscaped = false;
                continue;
            }

            if (ch == '\\')
            {
                isEscaped = true;
                continue;
            }

            output.Append(ch);
        }

        if (isEscaped)
        {
            output.Append('\\');
        }

        return output.ToString();
    }

    private static List<string> SplitEscaped(string value)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var isEscaped = false;

        foreach (var ch in value)
        {
            if (isEscaped)
            {
                current.Append('\\');
                current.Append(ch);
                isEscaped = false;
                continue;
            }

            if (ch == '\\')
            {
                isEscaped = true;
                continue;
            }

            if (ch == Separator)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        if (isEscaped)
        {
            current.Append('\\');
        }

        result.Add(current.ToString());
        return result;
    }
}
