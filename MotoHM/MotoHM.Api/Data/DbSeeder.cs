namespace MotoHM.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedPartCategoriesAsync(db);
    }

    private static async Task SeedPartCategoriesAsync(AppDbContext db)
    {
        // Cədvəldə artıq data varsa, heç nə əlavə etmir
        if (await db.PartCategories.AnyAsync())
            return;

        db.PartCategories.AddRange(
            new PartCategoryEntity { Icon = "🛞", Name = "Təkərlər" },
            new PartCategoryEntity { Icon = "🛠️", Name = "Mühərrik hissələri" },
            new PartCategoryEntity { Icon = "🛑", Name = "Əyləc sistemi" },
            new PartCategoryEntity { Icon = "⛓️", Name = "Zəncir və dişlilər" },
            new PartCategoryEntity { Icon = "💡", Name = "Elektrik və işıqlar" },
            new PartCategoryEntity { Icon = "🔧", Name = "Filtrlər və yağlar" });

        await db.SaveChangesAsync();
    }
}