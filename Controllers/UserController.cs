using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models.Entities;

namespace Pivot.Controllers;

// Sadece super admin'in erişebileceği kullanıcı yönetim ekranları.
// Yetki/rol matrisi sonraki iş; şu an gateway'de role check'i YOK — inline guard.
public class UserController : Controller
{
    private readonly PivotDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UserController(PivotDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private bool IsSuperAdmin() =>
        User.HasClaim(AuthController.SuperAdminClaimType, "true");

    private IActionResult? GuardSuperAdmin()
    {
        if (!IsSuperAdmin())
            return Forbid();
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (GuardSuperAdmin() is { } forbid) return forbid;
        var users = await _db.Users.OrderBy(u => u.Id).ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (GuardSuperAdmin() is { } forbid) return forbid;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string username, string email, string password, bool isSuperAdmin = false)
    {
        if (GuardSuperAdmin() is { } forbid) return forbid;

        username = (username ?? "").Trim();
        email = (email ?? "").Trim();
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
            IsSuperAdmin = isSuperAdmin,
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
        if (GuardSuperAdmin() is { } forbid) return forbid;
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        ViewData["UserId"] = user.Id;
        ViewData["Username"] = user.Username;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string password)
    {
        if (GuardSuperAdmin() is { } forbid) return forbid;
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

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
}
