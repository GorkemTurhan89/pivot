namespace Pivot.Models.Auth;

// Uygulama rolleri. Tek string kolonu (User.Role) — bir kullanıcı tek rol.
// Yeni rol eklemek için: buraya sabit ekle + UserController.AllRoles listesine al.
// Gateway korumaları [Authorize(Roles = Roles.X)] üzerinden çalışır.
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin"; // tüm yetkiler + kullanıcı yönetimi
    public const string Admin = "Admin";           // tüm iş aksiyonları (üye/sepet/teklif), kullanıcı yönetimi yok
    public const string SalesRep = "SalesRep";     // satış temsilcisi, iş aksiyonları

    public static readonly IReadOnlyList<string> All = new[] { SuperAdmin, Admin, SalesRep };
}
