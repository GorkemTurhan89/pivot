using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models.Auth;
using Pivot.Models.Entities;
using Pivot.Models.ViewModels;

namespace Pivot.Controllers;

// Fiyat yönetim ekranı: SuperAdmin + Admin. Cascading Country→City→School → listele;
// her satırın Edit'inde sadece fiyat alanları düzenlenebilir.
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class PriceController : Controller
{
    private readonly PivotDbContext _db;

    public PriceController(PivotDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var countries = await _db.Countries
            .OrderBy(c => c.Name)
            .Select(c => new LookupItem { Id = c.Id, Name = c.Name })
            .ToListAsync();
        return View(countries);
    }

    // Bir okuldaki tüm planlar (main + addon). Currency Country'den gelir.
    [HttpGet]
    public async Task<IActionResult> GetPlans(int schoolId)
    {
        var currency = await _db.Schools
            .Where(s => s.Id == schoolId)
            .Select(s => s.City!.Country!.Currency)
            .FirstOrDefaultAsync() ?? "";

        var plans = await _db.PaymentPlans
            .Where(p => p.SchoolId == schoolId)
            .OrderBy(p => p.IsAdditional).ThenBy(p => p.Category).ThenBy(p => p.Name)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name,
                programId = p.ProgramId,
                programName = p.Program != null ? p.Program.Name : null,
                minWeek = p.MinWeek,
                maxWeek = p.MaxWeek,
                priceType = p.PriceType.ToString(),
                isAdditional = p.IsAdditional,
                category = p.Category,
                weeklyListFee = p.WeeklyListFee,
                weeklyPromoFee = p.WeeklyPromoFee,
                totalListFee = p.TotalListFee,
                totalPromoFee = p.TotalPromoFee,
                registrationFee = p.RegistrationFee,
                membershipFee = p.MembershipFee,
                promotedFee = p.PromotedFee,
                promoValidFrom = p.PromoValidFrom,
                promoValidUntil = p.PromoValidUntil,
                isActive = p.IsActive,
                currency
            })
            .ToListAsync();

        return Json(plans);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var plan = await _db.PaymentPlans
            .Include(p => p.School)
                .ThenInclude(s => s!.City)
                    .ThenInclude(c => c!.Country)
            .Include(p => p.Program)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null) return NotFound();

        ViewData["Currency"] = plan.School?.City?.Country?.Currency ?? "";
        return View(plan);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PriceEditInput input)
    {
        var plan = await _db.PaymentPlans
            .Include(p => p.School)
                .ThenInclude(s => s!.City)
                    .ThenInclude(c => c!.Country)
            .Include(p => p.Program)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null) return NotFound();

        ViewData["Currency"] = plan.School?.City?.Country?.Currency ?? "";

        // Sadece fiyat alanlarını uygula; yapısal alanlar (School/Program/Min-Max/PriceType/PackageType/Category) dokunulmaz.
        plan.WeeklyListFee = input.WeeklyListFee;
        plan.WeeklyPromoFee = input.WeeklyPromoFee;
        plan.TotalListFee = input.TotalListFee;
        plan.TotalPromoFee = input.TotalPromoFee;
        plan.MembershipFee = input.MembershipFee;
        plan.RegistrationFee = input.RegistrationFee;
        plan.PromotedFee = input.PromotedFee;
        plan.PromoValidFrom = input.PromoValidFrom;
        plan.PromoValidUntil = input.PromoValidUntil;
        plan.IsActive = input.IsActive;

        await _db.SaveChangesAsync();
        TempData["Saved"] = $"#{plan.Id} güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public class PriceEditInput
    {
        public decimal? WeeklyListFee { get; set; }
        public decimal? WeeklyPromoFee { get; set; }
        public decimal? TotalListFee { get; set; }
        public decimal? TotalPromoFee { get; set; }
        public decimal? MembershipFee { get; set; }
        public decimal? RegistrationFee { get; set; }
        public decimal? PromotedFee { get; set; }
        public DateOnly? PromoValidFrom { get; set; }
        public DateOnly? PromoValidUntil { get; set; }
        public bool IsActive { get; set; }
    }
}
