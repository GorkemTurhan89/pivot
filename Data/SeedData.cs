using Microsoft.EntityFrameworkCore;

namespace Pivot.Data;

// BÜYÜK GEÇİŞ (2026-05-30): mock seed kaldırıldı.
// Gerçek veri Excel'den Python import script'i ile yükleniyor (backups/import_excel.py).
// Bu sınıf artık sadece migration koşturur; veri tohumlama yapmaz.
public static class SeedData
{
    public static async Task InitializeAsync(PivotDbContext db)
    {
        await db.Database.MigrateAsync();
    }
}
