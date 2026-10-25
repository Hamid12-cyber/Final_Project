namespace MotoHM.Api.Shared.Email;

public static class EmailLanguage
{
    public const string Default = "az";
    private static readonly string[] Supported = ["az", "en", "ru"];

    public static string FromRequest(HttpRequest request)
    {
        var header = request.Headers.AcceptLanguage.ToString();

        foreach (var part in header.Split(','))
        {
            var tag = part.Split(';')[0].Trim().ToLowerInvariant();
            if (tag.Length < 2) continue;

            var lang = tag[..2];
            if (Supported.Contains(lang)) return lang;
        }

        return Default;
    }
}