using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pivot.Data;
using Pivot.Models.Entities;

namespace Pivot.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    public const string TokenCookieName = "access_token";
    public const string SuperAdminClaimType = "is_super_admin";

    private readonly IConfiguration _config;
    private readonly PivotDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AuthController(IConfiguration config, PivotDbContext db, IPasswordHasher<User> hasher)
    {
        _config = config;
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Login: username VEYA email + password. PasswordHasher rehash önerirse hash'i yeniler.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string usernameOrEmail, string password, string? returnUrl = null)
    {
        usernameOrEmail = (usernameOrEmail ?? "").Trim();
        if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı adı/e-posta ve şifre zorunlu.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        if (user != null)
        {
            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success ||
                result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    user.PasswordHash = _hasher.HashPassword(user, password);
                    user.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }

                var token = IssueToken(user, out var expires);
                Response.Cookies.Append(TokenCookieName, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = expires
                });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
        }

        ModelState.AddModelError(string.Empty, "Kullanıcı adı/e-posta veya şifre hatalı.");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(TokenCookieName);
        return RedirectToAction(nameof(Login));
    }

    private string IssueToken(User user, out DateTimeOffset expiresAt)
    {
        var jwt = _config.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwt["SigningKey"]!);
        var expiryMinutes = int.Parse(jwt["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(SuperAdminClaimType, user.IsSuperAdmin ? "true" : "false")
        };

        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256));

        expiresAt = new DateTimeOffset(expiry);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
