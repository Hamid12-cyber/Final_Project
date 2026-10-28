
namespace MotoHM.Api.Shared.Email;

public enum EmailPurpose
{
    VerifyEmail,
    ResetPassword
}

public record EmailContent(string Subject, string Body);

public static class EmailTemplates
{
    private static readonly Lazy<string> CodeTemplate = new(() => Load("CodeEmail.html"));

    public static EmailContent Code(EmailPurpose purpose, string? lang, string fullName, string code, int validMinutes)
    {
        var t = EmailTexts.Get(lang);

        var (subject, heading, intro) = purpose == EmailPurpose.VerifyEmail
            ? (t.VerifySubject, t.VerifyHeading, t.VerifyIntro)
            : (t.ResetSubject, t.ResetHeading, t.ResetIntro);

        static string E(string s) => WebUtility.HtmlEncode(s);

        var body = CodeTemplate.Value
            .Replace("{{Lang}}", t.Lang)
            .Replace("{{Heading}}", E(heading))
            .Replace("{{Preview}}", E(string.Format(t.Preview, code, validMinutes)))
            .Replace("{{Greeting}}", E(t.Greeting))
            .Replace("{{Intro}}", E(intro))
            .Replace("{{Code}}", E(code))
            .Replace("{{ValidText}}", E(string.Format(t.ValidText, validMinutes)))
            .Replace("{{SecurityTitle}}", E(t.SecurityTitle))
            .Replace("{{SecurityText}}", E(t.SecurityText))
            .Replace("{{Footer}}", E(t.Footer))
            .Replace("{{Name}}", E(fullName));   // istifadeci daxil etdiyi menbe en sonda

        return new EmailContent(subject, body);
    }

    private static string Load(string fileName)
    {
        var assembly = typeof(EmailTemplates).Assembly;

        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("." + fileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Email şablonu tapılmadı: {fileName}");

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}