using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pivot.Models.Entities;

namespace Pivot.Data;

// BÜYÜK GEÇİŞ (2026-05-30): mock veri seed'i kaldırıldı. Gerçek katalog Excel'den
// import script'leri ile yükleniyor. Bu sınıf migration koşturur ve ilk super admin'i
// yoksa oluşturur (auth iskelet).
public static class SeedData
{
    public static async Task InitializeAsync(PivotDbContext db)
    {
        await db.Database.MigrateAsync();
        await EnsureSuperAdminAsync(db);
    }

    private static async Task EnsureSuperAdminAsync(PivotDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.IsSuperAdmin)) return;

        var user = new User
        {
            Username = "admin",
            Email = "admin@admin.com",
            IsSuperAdmin = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        // İlk super admin şifresi: adminGorkem1989 — kullanıcı login sonrası değiştirebilir.
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "adminGorkem1989");

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
