using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models.Entities;
using Pivot.Models.ViewModels;

namespace Pivot.Controllers;

public class SearchController : Controller
{
    private readonly PivotDbContext _db;

    public SearchController(PivotDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new SearchIndexViewModel
        {
            Countries = await _db.Countries
                .OrderBy(c => c.Name)
                .Select(c => new LookupItem { Id = c.Id, Name = c.Name })
                .ToListAsync()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetCities(int countryId)
    {
        var cities = await _db.Cities
            .Where(c => c.CountryId == countryId)
            .OrderBy(c => c.Name)
            .Select(c => new LookupItem { Id = c.Id, Name = c.Name })
            .ToListAsync();
        return Json(cities);
    }

    [HttpGet]
    public async Task<IActionResult> GetSchools(int cityId)
    {
        var schools = await _db.Schools
            .Where(s => s.CityId == cityId)
            .OrderBy(s => s.Name)
            .Select(s => new LookupItem { Id = s.Id, Name = s.Name })
            .ToListAsync();
        return Json(schools);
    }

    [HttpGet]
    public async Task<IActionResult> GetPrograms(int schoolId)
    {
        var programs = await _db.Programs
            .Where(p => p.SchoolId == schoolId)
            .OrderBy(p => p.Name)
            .Select(p => new LookupItem { Id = p.Id, Name = p.Name })
            .ToListAsync();
        return Json(programs);
    }

    // BÜYÜK GEÇİŞ (2026-05-30): bant fiyat modeline geçildi.
    // Course'un kendi sezon geçerliliği (ValidFrom/ValidTo) ARTIK YOK — Excel'de yok.
    // weeks verilirse o bantta (MinWeek<=weeks<=MaxWeek) eşleşen plan dönülür ve toplamlar hesaplanır.
    [HttpGet]
    public async Task<IActionResult> GetPaymentPlans(int programId, DateOnly startDate, int? weeks)
    {
        var q = _db.PaymentPlans
            .Where(p => p.ProgramId == programId
                && p.PackageType == PackageType.Main
                && p.IsActive);

        if (weeks.HasValue && weeks.Value > 0)
        {
            var w = weeks.Value;
            q = q.Where(p => p.MinWeek <= w && w <= p.MaxWeek);
        }

        var raw = await q
            .OrderBy(p => p.MinWeek)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.MinWeek,
                p.MaxWeek,
                p.PriceType,
                p.WeeklyListFee,
                p.WeeklyPromoFee,
                p.TotalListFee,
                p.TotalPromoFee,
                Currency = p.School.City.Country.Currency
            })
            .ToListAsync();

        var plans = raw.Select(p =>
        {
            decimal listUnit;
            decimal? promoUnit;
            decimal? listTotal = null;
            decimal? promoTotal = null;
            if (p.PriceType == PriceType.Weekly)
            {
                listUnit = p.WeeklyListFee ?? 0m;
                promoUnit = p.WeeklyPromoFee;
                if (weeks.HasValue && weeks.Value > 0)
                {
                    listTotal = listUnit * weeks.Value;
                    if (promoUnit.HasValue) promoTotal = promoUnit.Value * weeks.Value;
                }
            }
            else
            {
                listUnit = p.TotalListFee ?? 0m;
                promoUnit = p.TotalPromoFee;
                listTotal = listUnit;
                promoTotal = promoUnit;
            }
            return new PaymentPlanListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                MinWeek = p.MinWeek,
                MaxWeek = p.MaxWeek,
                PriceType = p.PriceType.ToString(),
                ListUnit = listUnit,
                PromoUnit = promoUnit,
                SelectedWeeks = weeks,
                ListTotal = listTotal,
                PromoTotal = promoTotal,
                Currency = p.Currency,
                EndDate = weeks.HasValue && weeks.Value > 0 ? startDate.AddDays(weeks.Value * 7) : (DateOnly?)null
            };
        }).ToList();

        return Json(plans);
    }

    public IActionResult AddOns()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetAddOns(int schoolId, int mainPlanId)
    {
        // Okul bazlı ek paketler: ya bu ana pakete bağlı (link var) ya da hiçbir ana pakete
        // bağlı olmayan okul geneli opsiyoneller. Başka bir varyanta özel bağlı olanlar dışlanır.
        var addOns = await _db.PaymentPlans
            .Where(p => p.IsAdditional
                && p.IsActive
                && p.SchoolId == schoolId
                && (_db.MainAddOnLinks.Any(l => l.AddOnPlanId == p.Id && l.MainPlanId == mainPlanId)
                    || !_db.MainAddOnLinks.Any(l => l.AddOnPlanId == p.Id)))
            .OrderByDescending(p => p.IsOrHasMandatory)
            .ThenBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new AddOnListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Items.Sum(i => i.ItemPrice * i.Quantity),
                Currency = p.Items.Select(i => i.Currency).FirstOrDefault() ?? "GBP",
                IsMandatory = p.IsOrHasMandatory
                    && _db.MainAddOnLinks.Any(l => l.AddOnPlanId == p.Id && l.MainPlanId == mainPlanId)
            })
            .ToListAsync();

        return Json(addOns);
    }

    public IActionResult Cart()
    {
        return View();
    }

    // BÜYÜK GEÇİŞ (2026-05-30): kurs tutarı bant fiyatından hesaplanıyor.
    // weeks parametresi zorunlu — kullanıcı seçim ekranında girdiği hafta sayısı.
    [HttpGet]
    public async Task<IActionResult> GetCart(int mainPlanId, int weeks, [FromQuery] List<int> addOnIds)
    {
        var main = await _db.PaymentPlans
            .Where(p => p.Id == mainPlanId && p.PackageType == PackageType.Main)
            .Select(p => new
            {
                p.Name,
                p.PriceType,
                p.WeeklyListFee,
                p.WeeklyPromoFee,
                p.TotalListFee,
                p.TotalPromoFee,
                p.RegistrationFee,
                Currency = p.School.City.Country.Currency
            })
            .FirstOrDefaultAsync();

        if (main == null)
            return NotFound();

        decimal courseList;
        decimal courseFinal;
        decimal weeklyRate;
        if (main.PriceType == PriceType.Weekly)
        {
            var listFee = main.WeeklyListFee ?? 0m;
            var promoFee = main.WeeklyPromoFee;
            courseList = listFee * weeks;
            courseFinal = (promoFee ?? listFee) * weeks;
            weeklyRate = promoFee ?? listFee;
        }
        else
        {
            var listTotal = main.TotalListFee ?? 0m;
            var promoTotal = main.TotalPromoFee;
            courseList = listTotal;
            courseFinal = promoTotal ?? listTotal;
            weeklyRate = weeks > 0 ? courseFinal / weeks : courseFinal;
        }
        var discount = courseList - courseFinal;

        // BÜYÜK GEÇİŞ uzantısı: AddOn'lar artık bant fiyatlı (WeeklyListFee × weeks) ya da Total.
        // RegistrationFee bu addon seçilince eklenen kerelik bedel (örn. konaklama yerleştirme ücreti).
        var rawAddOns = await _db.PaymentPlans
            .Where(p => addOnIds.Contains(p.Id) && p.IsAdditional)
            .OrderByDescending(p => p.IsOrHasMandatory)
            .ThenBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(a => new
            {
                a.Name,
                a.Category,
                a.PriceType,
                a.WeeklyListFee,
                a.WeeklyPromoFee,
                a.TotalListFee,
                a.TotalPromoFee,
                a.RegistrationFee,
                a.IsOrHasMandatory
            })
            .ToListAsync();

        var addOnLines = rawAddOns.Select(a =>
        {
            decimal amount;
            if (a.PriceType == PriceType.Weekly)
                amount = (a.WeeklyPromoFee ?? a.WeeklyListFee ?? 0m) * weeks;
            else
                amount = a.TotalPromoFee ?? a.TotalListFee ?? 0m;
            return new CartAddOnLine
            {
                Name = a.Name,
                Category = a.Category,
                Amount = amount,
                RegistrationFee = a.RegistrationFee,
                IsMandatory = a.IsOrHasMandatory
            };
        }).ToList();

        var registration = main.RegistrationFee ?? 0m;
        var total = courseFinal + registration
                    + addOnLines.Sum(l => l.Amount + (l.RegistrationFee ?? 0m));

        var vm = new CartViewModel
        {
            Main = new CartMainLine
            {
                Name = main.Name,
                Weeks = weeks,
                WeeklyRate = weeklyRate,
                LineTotal = courseFinal,
                ListTotal = discount > 0 ? (decimal?)courseList : null,
                Discount = discount > 0 ? (decimal?)discount : null
            },
            RegistrationFee = registration > 0 ? registration : null,
            AddOns = addOnLines,
            Total = total,
            Currency = main.Currency
        };

        return Json(vm);
    }
}
