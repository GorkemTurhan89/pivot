namespace Pivot.Models.Entities;

// App kullanıcısı (auth). Lead/Member ile karıştırılmasın; bu tablo sadece app'e
// giriş yapanlar için. PasswordHash = ASP.NET PasswordHasher<User> PBKDF2.
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // SuperAdmin = kullanıcı yönetebilen tek role. Rol/yetki matrisi sonraki iş;
    // şu an gateway'de role check'i YOK, sadece UserController'da inline kontrol.
    public bool IsSuperAdmin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
