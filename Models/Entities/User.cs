namespace Pivot.Models.Entities;

// App kullanıcısı (auth). Lead/Member ile karıştırılmasın; bu tablo sadece app'e
// giriş yapanlar için. PasswordHash = ASP.NET PasswordHasher<User> PBKDF2.
// Role = Pivot.Models.Auth.Roles sabitlerinden biri (SuperAdmin/Admin/SalesRep).
public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
