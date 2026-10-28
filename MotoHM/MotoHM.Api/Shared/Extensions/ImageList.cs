namespace MotoHM.Api.Shared.Extensions;

public static class ImageList
{
    public const int MaxImages = 5;  
    public static List<string> Normalize(IEnumerable<string>? urls, string? fallbackSingle = null)
    {
        var list = (urls ?? Enumerable.Empty<string>())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct()
            .Take(MaxImages)
            .ToList();

        if (list.Count == 0 && !string.IsNullOrWhiteSpace(fallbackSingle))
            list.Add(fallbackSingle.Trim());

        return list;
    }

    public static string? ToJson(IReadOnlyCollection<string> urls) =>
        urls.Count == 0 ? null : JsonSerializer.Serialize(urls);
    public static List<string> FromJson(string? json, string? fallbackSingle = null)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(json);
                if (list is { Count: > 0 }) return list;
            }
            catch (JsonException) { }
        }

        return string.IsNullOrWhiteSpace(fallbackSingle) ? new List<string>() : new List<string> { fallbackSingle };
    }
}