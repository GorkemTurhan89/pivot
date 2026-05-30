using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models.Entities;

namespace Pivot.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class LeadController : Controller
{
    private readonly PivotDbContext _db;

    public LeadController(PivotDbContext db)
    {
        _db = db;
    }

    // /, /leadRegister, /Lead/Register hepsi buraya çıksın.
    [HttpGet("/")]
    [HttpGet("/leadRegister")]
    [HttpGet]   // /Lead/Register
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] MemberDetail? input)
    {
        if (input == null)
            return BadRequest(new { error = "Boş veya geçersiz JSON gövdesi." });

        input.FirstName = (input.FirstName ?? "").Trim();
        input.LastName = (input.LastName ?? "").Trim();
        input.Nationality = (input.Nationality ?? "").Trim();
        input.PersonalId = string.IsNullOrWhiteSpace(input.PersonalId) ? null : input.PersonalId.Trim();
        input.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
        input.PhoneNumber = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim();

        if (string.IsNullOrEmpty(input.FirstName) || string.IsNullOrEmpty(input.LastName)
            || string.IsNullOrEmpty(input.Nationality))
            return BadRequest(new { error = "Ad, Soyad ve Uyruk zorunludur." });

        // Uniqueness pre-check; çakışırsa mevcut kaydı döner (popup'ta Detay için).
        if (!string.IsNullOrEmpty(input.Email))
        {
            var existing = await _db.MemberDetails.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Email == input.Email);
            if (existing != null)
                return Conflict(new { duplicateField = "email", existingMember = existing });
        }
        if (!string.IsNullOrEmpty(input.PhoneNumber))
        {
            var existing = await _db.MemberDetails.AsNoTracking()
                .FirstOrDefaultAsync(m => m.PhoneNumber == input.PhoneNumber);
            if (existing != null)
                return Conflict(new { duplicateField = "phone", existingMember = existing });
        }

        input.Id = 0;
        input.CreatedAt = DateTime.UtcNow;
        _db.MemberDetails.Add(input);
        await _db.SaveChangesAsync();
        return Ok(new { memberId = input.Id });
    }
}
