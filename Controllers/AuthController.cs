using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Pivot.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private const string TokenCookieName = "access_token";

    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // STUB: gerçek Users tablosu sonraki alt-adımda gelecek. Şimdilik hardcoded admin/admin.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string username, string password, string? returnUrl = null)
    {
        if (username == "admin" && password == "admin")
        {
            var token = IssueToken(username, role: "Admin", out var expires);
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

        ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
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

    private string IssueToken(string username, string role, out DateTimeOffset expiresAt)
    {
        var jwt = _config.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwt["SigningKey"]!);
        var expiryMinutes = int.Parse(jwt["ExpiryMinutes"] ?? "60");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
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
