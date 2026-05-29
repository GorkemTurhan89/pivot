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

    [HttpGet]
    public async Task<IActionResult> GetPaymentPlans(int programId, DateOnly startDate)
    {
        var plans = await _db.PaymentPlans
            .Where(p => p.ProgramId == programId
                && p.PackageType == PackageType.Main
                && p.IsActive
                && p.ValidFrom <= startDate
                && startDate <= p.ValidTo
                && startDate.AddDays(p.LengthWeeks * 7) <= p.ValidTo)
            .OrderBy(p => p.LengthWeeks)
            .Select(p => new PaymentPlanListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                LengthWeeks = p.LengthWeeks,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                TotalPrice = p.Items.Sum(i => i.ItemPrice * i.Quantity),
                Currency = p.Items.Select(i => i.Currency).FirstOrDefault() ?? "GBP",
                EndDate = startDate.AddDays(p.LengthWeeks * 7)
            })
            .ToListAsync();

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

    [HttpGet]
    public async Task<IActionResult> GetCart(int mainPlanId, [FromQuery] List<int> addOnIds)
    {
        var main = await _db.PaymentPlans
            .Where(p => p.Id == mainPlanId && p.PackageType == PackageType.Main)
            .Select(p => new
            {
                p.Name,
                p.LengthWeeks,
                p.RegistrationFee,
                CourseSum = p.Items.Sum(i => i.ItemPrice * i.Quantity),
                Currency = p.Items.Select(i => i.Currency).FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (main == null)
            return NotFound();

        var addOnLines = await _db.PaymentPlans
            .Where(p => addOnIds.Contains(p.Id) && p.IsAdditional)
            .OrderByDescending(p => p.IsOrHasMandatory)
            .ThenBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(a => new CartAddOnLine
            {
                Name = a.Name,
                Category = a.Category,
                Amount = a.Items.Sum(i => i.ItemPrice * i.Quantity),
                IsMandatory = a.IsOrHasMandatory
            })
            .ToListAsync();

        var registration = main.RegistrationFee ?? 0m;
        var total = main.CourseSum + registration + addOnLines.Sum(l => l.Amount);

        var vm = new CartViewModel
        {
            Main = new CartMainLine
            {
                Name = main.Name,
                Weeks = main.LengthWeeks,
                WeeklyRate = main.LengthWeeks > 0 ? main.CourseSum / main.LengthWeeks : main.CourseSum,
                LineTotal = main.CourseSum
            },
            RegistrationFee = registration > 0 ? registration : null,
            AddOns = addOnLines,
            Total = total,
            Currency = main.Currency ?? "GBP"
        };

        return Json(vm);
    }
}
