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
                p.PromoValidFrom,
                p.PromoValidUntil,
                Currency = p.School.City.Country.Currency
            })
            .ToListAsync();

        var plans = raw.Select(p =>
        {
            // Kayıt tarihi (startDate) promo penceresi içinde değilse promo gösterilmez.
            bool inPromoWindow =
                (!p.PromoValidFrom.HasValue || p.PromoValidFrom.Value <= startDate)
                && (!p.PromoValidUntil.HasValue || startDate <= p.PromoValidUntil.Value);

            decimal listUnit;
            decimal? promoUnit;
            decimal? listTotal = null;
            decimal? promoTotal = null;
            if (p.PriceType == PriceType.Weekly)
            {
                listUnit = p.WeeklyListFee ?? 0m;
                promoUnit = inPromoWindow ? p.WeeklyPromoFee : null;
                if (weeks.HasValue && weeks.Value > 0)
                {
                    listTotal = listUnit * weeks.Value;
                    if (promoUnit.HasValue) promoTotal = promoUnit.Value * weeks.Value;
                }
            }
            else
            {
                listUnit = p.TotalListFee ?? 0m;
                promoUnit = inPromoWindow ? p.TotalPromoFee : null;
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
        // BÜYÜK GEÇİŞ sonrası: addon fiyatları bant alanlarında (Weekly: WeeklyListFee, FixedTotal: TotalListFee);
        // currency Country.Currency'den. Eski Items.Sum mantığı ÖLDÜ (Excel-import addon'larda item yok).
        var raw = await _db.PaymentPlans
            .Where(p => p.IsAdditional
                && p.IsActive
                && p.SchoolId == schoolId
                && p.Category != "Supplement"   // Supplement'lar accommodation seçilince dinamik gelir.
                && (_db.MainAddOnLinks.Any(l => l.AddOnPlanId == p.Id && l.MainPlanId == mainPlanId)
                    || !_db.MainAddOnLinks.Any(l => l.AddOnPlanId == p.Id)))
            .OrderByDescending(p => p.IsOrHasMandatory)
            .ThenBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Category,
                p.PriceType,
                p.WeeklyListFee,
                p.TotalListFee,
                Currency = p.School.City.Country.Currency,
                IsMandatoryForThisMain = p.IsOrHasMandatory
                    && _db.MainAddOnLinks.Any(l => l.AddOnPlanId == p.Id && l.MainPlanId == mainPlanId)
            })
            .ToListAsync();

        var addOns = raw.Select(p => new AddOnListItemViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            PriceType = p.PriceType.ToString(),
            Price = p.PriceType == PriceType.Weekly ? (p.WeeklyListFee ?? 0m) : (p.TotalListFee ?? 0m),
            Currency = p.Currency,
            IsMandatory = p.IsMandatoryForThisMain
        }).ToList();

        return Json(addOns);
    }

    // Bir accommodation seçildiğinde uygulanabilir supplement'ları döner.
    // Eşleşme: SupplementDetail.(School, Country, Campus, AppliesTo) ==
    //          AccommodationDetail.(School, Country, Campus, Type)
    [HttpGet]
    public async Task<IActionResult> GetSupplements(int accommodationId)
    {
        var acc = await _db.AccommodationDetails
            .Where(d => d.PaymentPlanId == accommodationId)
            .Select(d => new { d.School, d.Country, d.Campus, d.Type,
                               Currency = d.PaymentPlan.School.City.Country.Currency })
            .FirstOrDefaultAsync();

        if (acc == null) return Json(Array.Empty<SupplementListItemViewModel>());

        var supps = await _db.SupplementDetails
            .Where(s => s.School == acc.School
                     && s.Country == acc.Country
                     && s.Campus == acc.Campus
                     && s.AppliesTo == acc.Type
                     && s.PaymentPlan.IsActive)
            .OrderBy(s => s.SupplementName)
            .Select(s => new SupplementListItemViewModel
            {
                Id = s.PaymentPlanId,
                Name = s.SupplementName,
                AppliesTo = s.AppliesTo,
                WeeklyFee = s.PaymentPlan.WeeklyListFee ?? 0m,
                Currency = acc.Currency,
                IsConditional = s.Required == "Conditional",
                ConditionNotes = s.Notes,
                StartDate = s.StartDate,
                EndDate = s.EndDate
            })
            .ToListAsync();

        return Json(supps);
    }

    public IActionResult ExtraServices() => View();

    // ExtraServices (Vize/UçakBileti) seçilen ana paketin ülkesine göre listelenir.
    [HttpGet]
    public async Task<IActionResult> GetExtraServices(int countryId)
    {
        var countryName = await _db.Countries
            .Where(c => c.Id == countryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
        if (countryName == null) return Json(Array.Empty<ExtraServiceListItemViewModel>());

        var items = await (
            from p in _db.PaymentPlans
            join e in _db.ExtraServiceDetails on p.Id equals e.PaymentPlanId
            where p.IsActive && p.IsAdditional
                  && (p.Category == "Vize" || p.Category == "UçakBileti")
                  && e.Country == countryName
            orderby p.Category, p.Name
            select new ExtraServiceListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category ?? "",
                VisaType = e.VisaType,
                DefaultPrice = p.TotalListFee,
                DefaultPriceText = e.DefaultPriceText,
                Currency = e.Currency
            }).ToListAsync();

        return Json(items);
    }

    public IActionResult Cart()
    {
        return View();
    }

    // BÜYÜK GEÇİŞ (2026-05-30): kurs tutarı bant fiyatından hesaplanıyor.
    // weeks parametresi zorunlu — kullanıcı seçim ekranında girdiği hafta sayısı.
    [HttpGet]
    public async Task<IActionResult> GetCart(int mainPlanId, int weeks, DateOnly startDate,
        [FromQuery] List<int> addOnIds, [FromQuery] string? addOnWeeksJson = null,
        [FromQuery] string? extrasJson = null,
        [FromQuery] Guid? cartGuid = null)
    {
        // Weekly addon'ların hafta override'ları: {addonId: weeks} JSON. Yoksa ana paket hafta'sı kullanılır.
        var addOnWeeksMap = string.IsNullOrEmpty(addOnWeeksJson)
            ? new Dictionary<int, int>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(addOnWeeksJson)
                ?? new Dictionary<int, int>();

        // ExtraServices submit'inden gelen kalemler (UI'da tutarı düzenlenmiş + manuel eklenmiş).
        var extras = string.IsNullOrEmpty(extrasJson)
            ? new List<CartExtraInput>()
            : System.Text.Json.JsonSerializer.Deserialize<List<CartExtraInput>>(extrasJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new List<CartExtraInput>();

        var main = await _db.PaymentPlans
            .Where(p => p.Id == mainPlanId && p.PackageType == PackageType.Main)
            .Select(p => new
            {
                p.Name,
                p.MinWeek,
                p.MaxWeek,
                p.PriceType,
                p.WeeklyListFee,
                p.WeeklyPromoFee,
                p.TotalListFee,
                p.TotalPromoFee,
                p.PromoValidFrom,
                p.PromoValidUntil,
                p.RegistrationFee,
                Currency = p.School.City.Country.Currency
            })
            .FirstOrDefaultAsync();

        if (main == null)
            return NotFound();

        // Kayıt tarihi promo penceresi içinde değilse promo geçersiz; toplam liste fiyatından hesaplanır.
        bool inPromoWindow =
            (!main.PromoValidFrom.HasValue || main.PromoValidFrom.Value <= startDate)
            && (!main.PromoValidUntil.HasValue || startDate <= main.PromoValidUntil.Value);

        decimal courseList;
        decimal courseFinal;
        decimal weeklyRate;
        if (main.PriceType == PriceType.Weekly)
        {
            var listFee = main.WeeklyListFee ?? 0m;
            var promoFee = inPromoWindow ? main.WeeklyPromoFee : null;
            courseList = listFee * weeks;
            courseFinal = (promoFee ?? listFee) * weeks;
            weeklyRate = promoFee ?? listFee;
        }
        else
        {
            var listTotal = main.TotalListFee ?? 0m;
            var promoTotal = inPromoWindow ? main.TotalPromoFee : null;
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
                a.Id,
                a.Name,
                a.Category,
                a.MinWeek,
                a.MaxWeek,
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
            int usedWeeks = 0;
            if (a.PriceType == PriceType.Weekly)
            {
                // Per-addon override varsa kullan, yoksa ana paketin hafta'sına düş.
                usedWeeks = addOnWeeksMap.TryGetValue(a.Id, out var w) && w > 0 ? w : weeks;
                amount = (a.WeeklyPromoFee ?? a.WeeklyListFee ?? 0m) * usedWeeks;
            }
            else
            {
                amount = a.TotalPromoFee ?? a.TotalListFee ?? 0m;
            }
            return new CartAddOnLine
            {
                Name = a.Name,
                Category = a.Category,
                Amount = amount,
                Weeks = usedWeeks,
                RegistrationFee = a.RegistrationFee,
                IsMandatory = a.IsOrHasMandatory
            };
        }).ToList();

        var registration = main.RegistrationFee ?? 0m;

        // Extras: ekran tarafından gönderilen kalemler (UI'da tutarı düzenlenmiş + manuel).
        var extraLines = extras.Select(x => new CartAddOnLine
        {
            Name = x.Name,
            Category = x.Category,
            Amount = x.Amount,
            Weeks = 0,
            IsMandatory = false
        }).ToList();

        var total = courseFinal + registration
                    + addOnLines.Sum(l => l.Amount + (l.RegistrationFee ?? 0m))
                    + extraLines.Sum(l => l.Amount);

        // Cart üst kart bilgisi: cartGuid verildiyse müşteri özet bilgisi yüklenir.
        CartMemberInfo? memberInfo = null;
        if (cartGuid.HasValue)
        {
            memberInfo = await _db.CartDetails.AsNoTracking()
                .Where(c => c.CartGuid == cartGuid.Value)
                .Select(c => new CartMemberInfo
                {
                    FirstName = c.Member.FirstName,
                    LastName = c.Member.LastName,
                    Email = c.Member.Email,
                    Birthday = c.Member.Birthday
                })
                .FirstOrDefaultAsync();
        }

        // CRM snapshot: cartGuid varsa CartDetail OfferCreated'a evrilir ve
        // seçili planlar ChoosenPlanDetails'a yazılır (mevcut snapshot silinir, yenisi yazılır).
        // CartGuid (sequential int yerine) ileri/geri navigasyon arası aynı sepeti güncel tutar.
        if (cartGuid.HasValue)
        {
            var cart = await _db.CartDetails.Include(c => c.ChoosenPlans)
                .FirstOrDefaultAsync(c => c.CartGuid == cartGuid.Value);
            if (cart != null)
            {
                cart.Status = CartStatus.OfferCreated;
                cart.UpdateDate = DateTime.UtcNow;
                cart.TotalPaymentPrice = total;
                if (cart.ChoosenPlans.Count > 0) _db.ChoosenPlanDetails.RemoveRange(cart.ChoosenPlans);

                bool mainDiscounted = (main.PriceType == PriceType.Weekly
                        ? (inPromoWindow && main.WeeklyPromoFee.HasValue)
                        : (inPromoWindow && main.TotalPromoFee.HasValue));

                _db.ChoosenPlanDetails.Add(new ChoosenPlanDetail
                {
                    CartDetailId = cart.Id,
                    PaymentPlanId = mainPlanId,
                    Name = main.Name,
                    Category = null,
                    MinWeek = main.MinWeek,
                    MaxWeek = main.MaxWeek,
                    PriceType = main.PriceType,
                    WeeklyListFee = main.WeeklyListFee,
                    WeeklyPromoFee = main.WeeklyPromoFee,
                    TotalListFee = main.TotalListFee,
                    TotalPromoFee = main.TotalPromoFee,
                    SelectedWeeks = main.PriceType == PriceType.Weekly ? weeks : 0,
                    LineAmount = courseFinal,
                    RegistrationFee = main.RegistrationFee,
                    Currency = main.Currency,
                    IsDiscounted = mainDiscounted,
                    IsMandatory = false,
                    IsMain = true
                });

                foreach (var a in rawAddOns)
                {
                    int addonWeeks;
                    decimal addonAmount;
                    bool addonDiscounted;
                    if (a.PriceType == PriceType.Weekly)
                    {
                        addonWeeks = addOnWeeksMap.TryGetValue(a.Id, out var w) && w > 0 ? w : weeks;
                        var unit = a.WeeklyPromoFee ?? a.WeeklyListFee ?? 0m;
                        addonAmount = unit * addonWeeks;
                        addonDiscounted = a.WeeklyPromoFee.HasValue;
                    }
                    else
                    {
                        addonWeeks = 0;
                        addonAmount = a.TotalPromoFee ?? a.TotalListFee ?? 0m;
                        addonDiscounted = a.TotalPromoFee.HasValue;
                    }

                    _db.ChoosenPlanDetails.Add(new ChoosenPlanDetail
                    {
                        CartDetailId = cart.Id,
                        PaymentPlanId = a.Id,
                        Name = a.Name,
                        Category = a.Category,
                        MinWeek = a.MinWeek,
                        MaxWeek = a.MaxWeek,
                        PriceType = a.PriceType,
                        WeeklyListFee = a.WeeklyListFee,
                        WeeklyPromoFee = a.WeeklyPromoFee,
                        TotalListFee = a.TotalListFee,
                        TotalPromoFee = a.TotalPromoFee,
                        SelectedWeeks = addonWeeks,
                        LineAmount = addonAmount,
                        RegistrationFee = a.RegistrationFee,
                        Currency = main.Currency,
                        IsDiscounted = addonDiscounted,
                        IsMandatory = a.IsOrHasMandatory,
                        IsMain = false
                    });
                }

                // Extras (ExtraServices + manuel kalemler) snapshot'a yazılır.
                // PaymentPlanId manuel için NULL; ExtraService için PaymentPlan id'si.
                foreach (var x in extras)
                {
                    _db.ChoosenPlanDetails.Add(new ChoosenPlanDetail
                    {
                        CartDetailId = cart.Id,
                        PaymentPlanId = x.Id,    // manuel -> null
                        Name = x.Name,
                        Category = x.Category,
                        MinWeek = 1,
                        MaxWeek = 1,
                        PriceType = PriceType.FixedTotal,
                        TotalListFee = x.Amount,
                        SelectedWeeks = 0,
                        LineAmount = x.Amount,
                        Currency = string.IsNullOrEmpty(x.Currency) ? main.Currency : x.Currency,
                        IsDiscounted = false,
                        IsMandatory = false,
                        IsMain = false
                    });
                }

                await _db.SaveChangesAsync();
            }
        }

        var vm = new CartViewModel
        {
            Member = memberInfo,
            StartDate = startDate,
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
            Extras = extraLines,
            Total = total,
            Currency = main.Currency
        };

        return Json(vm);
    }

    // Fiyat teklifi (Quote) — Cart sayfasından "PDF Görüntüle" ya da "PDF İndir" ile çağrılır.
    // print=true ise view JS ile window.print() tetikler (browser → Save as PDF).
    [HttpGet]
    public async Task<IActionResult> Quote(Guid cartGuid, bool print = false)
    {
        var cart = await _db.CartDetails
            .AsNoTracking()
            .Include(c => c.Member)
            .Include(c => c.ChoosenPlans)
            .FirstOrDefaultAsync(c => c.CartGuid == cartGuid);
        if (cart == null) return NotFound();

        var mainPlan = cart.ChoosenPlans.FirstOrDefault(p => p.IsMain);
        string schoolName = "", programName = "", cityName = "", countryName = "";
        if (mainPlan?.PaymentPlanId is int mainPlanId)
        {
            var info = await _db.PaymentPlans.AsNoTracking()
                .Where(p => p.Id == mainPlanId)
                .Select(p => new
                {
                    School = p.School!.Name,
                    Program = p.Program != null ? p.Program.Name : null,
                    City = p.School!.City!.Name,
                    Country = p.School!.City!.Country!.Name
                })
                .FirstOrDefaultAsync();
            if (info != null)
            {
                schoolName = info.School;
                programName = info.Program ?? "";
                cityName = info.City;
                countryName = info.Country;
            }
        }

        var currency = mainPlan?.Currency
            ?? cart.ChoosenPlans.FirstOrDefault()?.Currency
            ?? "";

        var vm = new QuoteViewModel
        {
            CartGuid = cartGuid,
            CreatedAt = cart.UpdateDate,
            AutoPrint = print,

            CustomerFullName = $"{cart.Member.FirstName} {cart.Member.LastName}".Trim(),
            CustomerEmail = cart.Member.Email,
            CustomerPhone = cart.Member.PhoneNumber,
            CustomerNationality = cart.Member.Nationality,
            CustomerBirthday = cart.Member.Birthday,

            SalesRepName = User.Identity?.Name ?? "",
            SalesRepEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            SalesRepRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "",

            SchoolName = schoolName,
            CityName = cityName,
            CountryName = countryName,
            ProgramName = programName,
            MainWeeks = mainPlan?.SelectedWeeks ?? 0,
            Currency = currency,

            GrandTotal = cart.TotalPaymentPrice
        };

        // Kategorize: Ana paket + non-konaklama addon'lar => Course. Konaklama/Supplement => Accommodation.
        // Vize/Uçak veya PaymentPlanId == null (manuel) => Extras.
        bool IsExtra(ChoosenPlanDetail p) =>
            p.PaymentPlanId == null
            || string.Equals(p.Category, "Vize", StringComparison.OrdinalIgnoreCase)
            || string.Equals(p.Category, "Vize Ücreti", StringComparison.OrdinalIgnoreCase)
            || (p.Category?.Contains("Uçak", StringComparison.OrdinalIgnoreCase) ?? false);
        bool IsAccommodation(ChoosenPlanDetail p) =>
            string.Equals(p.Category, "Konaklama", StringComparison.OrdinalIgnoreCase)
            || string.Equals(p.Category, "Supplement", StringComparison.OrdinalIgnoreCase);

        if (mainPlan != null)
        {
            vm.CourseLines.Add(new QuoteLine
            {
                Label = mainPlan.Name,
                Detail = mainPlan.SelectedWeeks > 0 ? $"{mainPlan.SelectedWeeks} hafta" : null,
                Amount = mainPlan.LineAmount,
                Currency = mainPlan.Currency
            });
            if (mainPlan.RegistrationFee is decimal reg && reg > 0m)
            {
                vm.CourseLines.Add(new QuoteLine
                {
                    Label = "Kayıt Ücreti",
                    Amount = reg,
                    Currency = mainPlan.Currency
                });
            }
        }

        foreach (var a in cart.ChoosenPlans.Where(p => !p.IsMain))
        {
            var target = IsAccommodation(a)
                ? vm.AccommodationLines
                : IsExtra(a) ? vm.ExtraLines : vm.CourseLines;

            target.Add(new QuoteLine
            {
                Label = a.Name + (a.IsMandatory ? " (Zorunlu)" : ""),
                Detail = a.SelectedWeeks > 0 ? $"{a.SelectedWeeks} hafta" : a.Category,
                Amount = a.LineAmount,
                Currency = a.Currency
            });
            if (a.RegistrationFee is decimal addonReg && addonReg > 0m)
            {
                target.Add(new QuoteLine
                {
                    Label = $"↳ {a.Name} – Yerleştirme",
                    Amount = addonReg,
                    Currency = a.Currency
                });
            }
        }

        vm.CourseTotal = vm.CourseLines.Sum(l => l.Amount);
        vm.AccommodationTotal = vm.AccommodationLines.Sum(l => l.Amount);

        return View(vm);
    }
}
