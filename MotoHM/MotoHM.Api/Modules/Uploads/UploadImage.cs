using MotoHM.Api.Shared.Exceptions.Common;

namespace MotoHM.Api.Modules.Uploads;

public static class UploadImage
{
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        // JWT ilə qorunan API olduğu üçün antiforgery lazım deyil
        app.MapPost("/api/uploads/images", async (IFormFile file, HttpContext http, IWebHostEnvironment env, CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest, "Fayl seçilməyib.");

            if (file.Length > MaxFileSize)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Şəkil 5 MB-dan böyük ola bilməz.");

            // Faylın adına və ya göndərilən content-type-a yox, real məzmununa (ilk baytlarına) baxırıq
            var header = new byte[12];
            int read;
            await using (var probe = file.OpenReadStream())
            {
                read = await probe.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
            }

            var extension = DetectExtension(header.AsSpan(0, read));
            if (extension is null)
                throw new AppException("BUSINESS_RULE", StatusCodes.Status400BadRequest,
                    "Yalnız JPG, PNG və ya WebP şəkilləri qəbul olunur.");

            var uploadsPath = Path.Combine(env.ContentRootPath, "uploads");
            Directory.CreateDirectory(uploadsPath);

            // İstifadəçinin göndərdiyi fayl adı istifadə olunmur, təsadüfi ad veririk
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            await using (var input = file.OpenReadStream())
            await using (var output = File.Create(fullPath))
            {
                await input.CopyToAsync(output, ct);
            }

            var url = $"{http.Request.Scheme}://{http.Request.Host}/uploads/{fileName}";
            return Results.Ok(new { url });
        })
        .DisableAntiforgery()
        .RequireAuthorization()
        .WithName("UploadImage")
        .WithTags("Uploads");
    }

    private static string? DetectExtension(ReadOnlySpan<byte> h)
    {
        // JPEG: FF D8 FF
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF)
            return ".jpg";

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (h.Length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47 &&
            h[4] == 0x0D && h[5] == 0x0A && h[6] == 0x1A && h[7] == 0x0A)
            return ".png";

        // WebP: "RIFF" .... "WEBP"
        if (h.Length >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F' &&
            h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P')
            return ".webp";

        return null;
    }
}