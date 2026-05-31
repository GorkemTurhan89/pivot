using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models.Auth;
using Pivot.Models.Entities;

namespace Pivot.Controllers;

// Kullanıcı yönetimi: SuperAdmin ve Admin erişebilir. Yetki matrisi:
// - SuperAdmin: tek olmalı (seed'de gelen). Admin ve SalesRep yaratabilir/şifre sıfırlayabilir/rol değiştirebilir.
//   Hiç kimseye SuperAdmin rolü atayamaz (single-SuperAdmin invariant).
// - Admin: sadece SalesRep yaratabilir + SalesRep şifre sıfırlayabilir. Rol değiştirme yok.
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class UserController : Controller
{
    private readonly PivotDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UserController(PivotDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private bool IsSuperAdmin() => User.IsInRole(Roles.SuperAdmin);

    // Görüntüleyen rolün hedef kullanıcıya işlem yapabileceği rolleri döner.
    // SuperAdmin: Admin ve SalesRep (SuperAdmin değişmez — tek olmalı).
    // Admin: yalnız SalesRep.
    private IReadOnlyList<string> AssignableRoles() =>
        IsSuperAdmin()
            ? new[] { Roles.Admin, Roles.SalesRep }
            : new[] { Roles.SalesRep };

    private bool CanManage(string targetRole) =>
        IsSuperAdmin()
            ? targetRole != Roles.SuperAdmin   // SuperAdmin kendi rolüne dokunamaz / SuperAdmin yaratamaz
            : targetRole == Roles.SalesRep;     // Admin yalnız SalesRep'i yönetir

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.OrderBy(u => u.Id).ToListAsync();
        ViewData["IsSuperAdmin"] = IsSuperAdmin();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Roles"] = AssignableRoles();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string username, string email, string password, string role)
    {
        username = (username ?? "").Trim();
        email = (email ?? "").Trim();
        role = (role ?? "").Trim();

        var assignable = AssignableRoles();
        ViewData["Roles"] = assignable;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(string.Empty, "Tüm alanlar zorunlu.");
            return View();
        }
        if (password.Length < 8)
        {
            ModelState.AddModelError(string.Empty, "Şifre en az 8 karakter olmalı.");
            return View();
        }
        if (!assignable.Contains(role))
        {
            ModelState.AddModelError(string.Empty, "Bu rolü atama yetkin yok.");
            return View();
        }

        if (await _db.Users.AnyAsync(u => u.Username == username))
        {
            ModelState.AddModelError(string.Empty, "Bu kullanıcı adı zaten alınmış.");
            return View();
        }
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(string.Empty, "Bu e-posta zaten kayıtlı.");
            return View();
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (!CanManage(user.Role)) return Forbid();

        ViewData["UserId"] = user.Id;
        ViewData["Username"] = user.Username;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string password)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (!CanManage(user.Role)) return Forbid();

        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            ModelState.AddModelError(string.Empty, "Şifre en az 8 karakter olmalı.");
            ViewData["UserId"] = user.Id;
            ViewData["Username"] = user.Username;
            return View();
        }

        user.PasswordHash = _hasher.HashPassword(user, password);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // EditRole sadece SuperAdmin'e açık. SuperAdmin Admin↔SalesRep arası geçiş yapabilir.
    [Authorize(Roles = Roles.SuperAdmin)]
    [HttpGet]
    public async Task<IActionResult> EditRole(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Role == Roles.SuperAdmin) return Forbid();  // SuperAdmin rolüne dokunulmaz.

        ViewData["Roles"] = AssignableRoles();
        return View(user);
    }

    [Authorize(Roles = Roles.SuperAdmin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(int id, string role)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Role == Roles.SuperAdmin) return Forbid();

        role = (role ?? "").Trim();
        var assignable = AssignableRoles();
        if (!assignable.Contains(role))
        {
            ModelState.AddModelError(string.Empty, "Bu rolü atama yetkin yok.");
            ViewData["Roles"] = assignable;
            return View(user);
        }

        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
