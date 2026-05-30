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

    // /leadRegister ve /Lead/Register buraya çıksın (ana sayfa Home'da).
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

        // Uniqueness pre-check; çakışırsa mevcut kaydı döner (popup "Bu kayıt ile devam et" için).
        // Phone primary identifier — önce phone bakılır, sonra email (çakışırsa hangisi raporlanır net olsun).
        if (!string.IsNullOrEmpty(input.PhoneNumber))
        {
            var existing = await _db.MemberDetails.AsNoTracking()
                .FirstOrDefaultAsync(m => m.PhoneNumber == input.PhoneNumber);
            if (existing != null)
                return Conflict(new { duplicateField = "phone", existingMember = existing });
        }
        if (!string.IsNullOrEmpty(input.Email))
        {
            var existing = await _db.MemberDetails.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Email == input.Email);
            if (existing != null)
                return Conflict(new { duplicateField = "email", existingMember = existing });
        }

        input.Id = 0;
        input.CreatedAt = DateTime.UtcNow;
        _db.MemberDetails.Add(input);
        await _db.SaveChangesAsync();

        // CRM lead kaydı: üye yaratıldığı an CartDetail (LeadCreated) düşer.
        // Snapshot alanları ileride üyenin profili değişse de bu cart için sabit kalır.
        var cart = new CartDetail
        {
            MemberId = input.Id,
            PersonalId = input.PersonalId,
            RegisterDate = DateOnly.FromDateTime(input.CreatedAt),
            Nationality = input.Nationality,
            Email = input.Email,
            PhoneNumber = input.PhoneNumber,
            Status = CartStatus.LeadCreated,
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CartGuid = Guid.NewGuid(),
            TotalPaymentPrice = 0m
        };
        _db.CartDetails.Add(cart);
        await _db.SaveChangesAsync();

        return Ok(new { memberId = input.Id, cartDetailId = cart.Id, cartGuid = cart.CartGuid });
    }

    // Mevcut üye ile YENİ teklif başlat: member tablosuna dokunmadan (formdaki değerlerle
    // güncelleme YAPILMAZ — üye güncelleme ayrı bir ekranın işi) yeni bir CartDetail yaratır.
    [HttpPost]
    public async Task<IActionResult> UseExisting([FromQuery] int memberId)
    {
        var member = await _db.MemberDetails.FindAsync(memberId);
        if (member == null) return NotFound(new { error = "Üye bulunamadı." });

        var cart = new CartDetail
        {
            MemberId = member.Id,
            PersonalId = member.PersonalId,
            RegisterDate = DateOnly.FromDateTime(member.CreatedAt),
            Nationality = member.Nationality,
            Email = member.Email,
            PhoneNumber = member.PhoneNumber,
            Status = CartStatus.LeadCreated,
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            CartGuid = Guid.NewGuid(),
            TotalPaymentPrice = 0m
        };
        _db.CartDetails.Add(cart);
        await _db.SaveChangesAsync();

        return Ok(new { memberId = member.Id, cartDetailId = cart.Id, cartGuid = cart.CartGuid });
    }
}
