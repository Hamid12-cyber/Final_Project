namespace MotoHM.Api.Modules.Uploads;

public static class UploadImage
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/uploads/image", async (IFormFile file, IWebHostEnvironment env, HttpRequest request) =>
        {
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { message = "Fayl seçilməyib." });

            if (file.Length > MaxFileSizeBytes)
                return Results.BadRequest(new { message = "Şəkil 5 MB-dan böyük ola bilməz." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return Results.BadRequest(new { message = "Yalnız jpg, png, webp, gif formatları dəstəklənir." });

            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"{request.Scheme}://{request.Host}/uploads/{fileName}";
            return Results.Ok(new { url });
        })
        .DisableAntiforgery()
        .WithName("UploadImage")
        .WithTags("Uploads")
        .RequireAuthorization();
    }
}