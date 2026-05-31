using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pivot.Data;
using Pivot.Models.Auth;
using Pivot.Models.Entities;
using Pivot.Utilities;

namespace Pivot.Controllers;

// Tüm catalog veri export/import (Country/City/School/Program/PaymentPlan + ek tablolar).
// Üye/sepet/auth tablolarına dokunulmaz. SuperAdmin + Admin erişebilir.
[Authorize(Roles = $"{Roles.SuperAdmin},{Roles.Admin}")]
public class DataController : Controller
{
    private readonly PivotDbContext _db;

    public DataController(PivotDbContext db)
    {
        _db = db;
    }

    public IActionResult Index() => View();

    // Tek bir ZIP içinde her tablo için ayrı CSV; client browser'a stream'lenir.
    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteCountriesAsync(zip);
            await WriteCitiesAsync(zip);
            await WriteSchoolsAsync(zip);
            await WriteProgramsAsync(zip);
            await WritePaymentPlansAsync(zip);
            await WriteMainAddOnLinksAsync(zip);
            await WriteAccommodationDetailsAsync(zip);
            await WriteSupplementDetailsAsync(zip);
            await WriteExtraServiceDetailsAsync(zip);
        }
        ms.Position = 0;
        var fileName = $"pivot-catalog-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";
        return File(ms, "application/zip", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50_000_000)] // 50MB
    public async Task<IActionResult> Import(IFormFile? file, bool confirm)
    {
        if (!confirm)
        {
            TempData["Error"] = "Onay kutusunu işaretlemeden import yapılamaz.";
            return RedirectToAction(nameof(Index));
        }
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Dosya boş ya da seçilmedi.";
            return RedirectToAction(nameof(Index));
        }

        // Parse aşaması — DB'ye dokunmadan tüm satırları belleğe oku ki sonra atomic insert edebilelim.
        List<Country> countries; List<City> cities; List<School> schools; List<CourseProgram> programs;
        List<PaymentPlan> plans; List<MainAddOnLink> links;
        List<AccommodationDetail> accs; List<SupplementDetail> sups; List<ExtraServiceDetail> extras;
        try
        {
            using var stream = file.OpenReadStream();
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
            countries = ReadCountries(zip);
            cities = ReadCities(zip);
            schools = ReadSchools(zip);
            programs = ReadPrograms(zip);
            plans = ReadPaymentPlans(zip);
            links = ReadMainAddOnLinks(zip);
            accs = ReadAccommodationDetails(zip);
            sups = ReadSupplementDetails(zip);
            extras = ReadExtraServiceDetails(zip);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"ZIP parse hatası: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }

        // Transaction içinde truncate + insert; FK kontrolünü geçici kapat.
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _db.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=0");

            // DELETE (TRUNCATE değil) — MySQL'de TRUNCATE implicit commit yapar, rollback işe
            // yaramaz. DELETE DML olduğu için transaction'a uyar; hata olursa atomic geri alır.
            // Sepet/Member/Auth tablolarına DOKUNMA — sadece catalog.
            foreach (var t in new[] {
                "extraservicedetails", "supplementdetails", "accommodationdetails",
                "mainaddonlinks", "paymentplanitems", "paymentplans",
                "programs", "schools", "cities", "countries" })
            {
                await _db.Database.ExecuteSqlRawAsync($"DELETE FROM `{t}`");
            }

            // Insert forward order. Identity_insert için EF'in raw bulk insert'i değil,
            // attach + SaveChanges yeterli (Pomelo MySQL EF Core kendi yönetir).
            await _db.Countries.AddRangeAsync(countries);
            await _db.Cities.AddRangeAsync(cities);
            await _db.Schools.AddRangeAsync(schools);
            await _db.Programs.AddRangeAsync(programs);
            await _db.PaymentPlans.AddRangeAsync(plans);
            await _db.MainAddOnLinks.AddRangeAsync(links);
            await _db.AccommodationDetails.AddRangeAsync(accs);
            await _db.SupplementDetails.AddRangeAsync(sups);
            await _db.ExtraServiceDetails.AddRangeAsync(extras);
            await _db.SaveChangesAsync();

            await _db.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1");
            await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            try { await _db.Database.ExecuteSqlRawAsync("SET FOREIGN_KEY_CHECKS=1"); } catch { }
            TempData["Error"] = $"Import hatası, geri alındı: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }

        TempData["Saved"] =
            $"İçe aktarıldı: {countries.Count} ülke, {cities.Count} şehir, {schools.Count} okul, " +
            $"{programs.Count} program, {plans.Count} plan, {links.Count} addon link, " +
            $"{accs.Count} acc, {sups.Count} supplement, {extras.Count} extra.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- WRITE (export) ----------

    private static StreamWriter NewEntryWriter(ZipArchive zip, string name)
    {
        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
        return new StreamWriter(entry.Open(), new UTF8Encoding(false));
    }

    private async Task WriteCountriesAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "Countries.csv");
        CsvHelper.WriteRow(w, "Id", "Name", "Currency");
        foreach (var c in await _db.Countries.AsNoTracking().OrderBy(x => x.Id).ToListAsync())
            CsvHelper.WriteRow(w, c.Id, c.Name, c.Currency);
    }

    private async Task WriteCitiesAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "Cities.csv");
        CsvHelper.WriteRow(w, "Id", "Name", "CountryId");
        foreach (var c in await _db.Cities.AsNoTracking().OrderBy(x => x.Id).ToListAsync())
            CsvHelper.WriteRow(w, c.Id, c.Name, c.CountryId);
    }

    private async Task WriteSchoolsAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "Schools.csv");
        CsvHelper.WriteRow(w, "Id", "Name", "CityId");
        foreach (var s in await _db.Schools.AsNoTracking().OrderBy(x => x.Id).ToListAsync())
            CsvHelper.WriteRow(w, s.Id, s.Name, s.CityId);
    }

    private async Task WriteProgramsAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "Programs.csv");
        CsvHelper.WriteRow(w, "Id", "Name", "SchoolId");
        foreach (var p in await _db.Programs.AsNoTracking().OrderBy(x => x.Id).ToListAsync())
            CsvHelper.WriteRow(w, p.Id, p.Name, p.SchoolId);
    }

    private async Task WritePaymentPlansAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "PaymentPlans.csv");
        CsvHelper.WriteRow(w,
            "Id", "SchoolId", "ProgramId", "Name", "MinWeek", "MaxWeek", "PriceType",
            "WeeklyListFee", "WeeklyPromoFee", "TotalListFee", "TotalPromoFee",
            "PromoValidFrom", "PromoValidUntil",
            "PackageType", "IsAdditional", "Category",
            "MembershipFee", "RegistrationFee", "PromotedFee",
            "IsOrHasMandatory", "IsActive");
        foreach (var p in await _db.PaymentPlans.AsNoTracking().OrderBy(x => x.Id).ToListAsync())
        {
            CsvHelper.WriteRow(w,
                p.Id, p.SchoolId, p.ProgramId, p.Name, p.MinWeek, p.MaxWeek, p.PriceType,
                p.WeeklyListFee, p.WeeklyPromoFee, p.TotalListFee, p.TotalPromoFee,
                p.PromoValidFrom, p.PromoValidUntil,
                p.PackageType, p.IsAdditional, p.Category,
                p.MembershipFee, p.RegistrationFee, p.PromotedFee,
                p.IsOrHasMandatory, p.IsActive);
        }
    }

    private async Task WriteMainAddOnLinksAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "MainAddOnLinks.csv");
        CsvHelper.WriteRow(w, "Id", "MainPlanId", "AddOnPlanId");
        foreach (var l in await _db.MainAddOnLinks.AsNoTracking().OrderBy(x => x.Id).ToListAsync())
            CsvHelper.WriteRow(w, l.Id, l.MainPlanId, l.AddOnPlanId);
    }

    private async Task WriteAccommodationDetailsAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "AccommodationDetails.csv");
        CsvHelper.WriteRow(w,
            "PaymentPlanId", "Type", "RoomType", "Board", "ResidenceName",
            "School", "Country", "Campus", "Required", "DisplayOption");
        foreach (var a in await _db.AccommodationDetails.AsNoTracking().OrderBy(x => x.PaymentPlanId).ToListAsync())
        {
            CsvHelper.WriteRow(w,
                a.PaymentPlanId, a.Type, a.RoomType, a.Board, a.ResidenceName,
                a.School, a.Country, a.Campus, a.Required, a.DisplayOption);
        }
    }

    private async Task WriteSupplementDetailsAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "SupplementDetails.csv");
        CsvHelper.WriteRow(w,
            "PaymentPlanId", "School", "Country", "Campus", "AppliesTo",
            "SupplementName", "Required", "Notes", "StartDate", "EndDate");
        foreach (var s in await _db.SupplementDetails.AsNoTracking().OrderBy(x => x.PaymentPlanId).ToListAsync())
        {
            CsvHelper.WriteRow(w,
                s.PaymentPlanId, s.School, s.Country, s.Campus, s.AppliesTo,
                s.SupplementName, s.Required, s.Notes, s.StartDate, s.EndDate);
        }
    }

    private async Task WriteExtraServiceDetailsAsync(ZipArchive zip)
    {
        using var w = NewEntryWriter(zip, "ExtraServiceDetails.csv");
        CsvHelper.WriteRow(w,
            "PaymentPlanId", "Country", "VisaType", "DefaultPriceText", "Currency");
        foreach (var e in await _db.ExtraServiceDetails.AsNoTracking().OrderBy(x => x.PaymentPlanId).ToListAsync())
        {
            CsvHelper.WriteRow(w,
                e.PaymentPlanId, e.Country, e.VisaType, e.DefaultPriceText, e.Currency);
        }
    }

    // ---------- READ (import) ----------

    private static IEnumerable<string[]> ReadEntry(ZipArchive zip, string name)
    {
        var entry = zip.GetEntry(name)
            ?? throw new InvalidOperationException($"ZIP içinde {name} bulunamadı.");
        using var s = entry.Open();
        using var r = new StreamReader(s, new UTF8Encoding(false));
        bool first = true;
        foreach (var row in CsvHelper.ParseRows(r))
        {
            if (first) { first = false; continue; } // header'ı atla
            yield return row;
        }
    }

    private static List<Country> ReadCountries(ZipArchive zip) =>
        ReadEntry(zip, "Countries.csv").Select(r => new Country
        {
            Id = CsvHelper.ParseInt(r[0]),
            Name = r[1],
            Currency = r[2]
        }).ToList();

    private static List<City> ReadCities(ZipArchive zip) =>
        ReadEntry(zip, "Cities.csv").Select(r => new City
        {
            Id = CsvHelper.ParseInt(r[0]),
            Name = r[1],
            CountryId = CsvHelper.ParseInt(r[2])
        }).ToList();

    private static List<School> ReadSchools(ZipArchive zip) =>
        ReadEntry(zip, "Schools.csv").Select(r => new School
        {
            Id = CsvHelper.ParseInt(r[0]),
            Name = r[1],
            CityId = CsvHelper.ParseInt(r[2])
        }).ToList();

    private static List<CourseProgram> ReadPrograms(ZipArchive zip) =>
        ReadEntry(zip, "Programs.csv").Select(r => new CourseProgram
        {
            Id = CsvHelper.ParseInt(r[0]),
            Name = r[1],
            SchoolId = CsvHelper.ParseInt(r[2])
        }).ToList();

    private static List<PaymentPlan> ReadPaymentPlans(ZipArchive zip) =>
        ReadEntry(zip, "PaymentPlans.csv").Select(r => new PaymentPlan
        {
            Id = CsvHelper.ParseInt(r[0]),
            SchoolId = CsvHelper.ParseIntOrNull(r[1]),
            ProgramId = CsvHelper.ParseIntOrNull(r[2]),
            Name = r[3],
            MinWeek = CsvHelper.ParseInt(r[4]),
            MaxWeek = CsvHelper.ParseInt(r[5]),
            PriceType = CsvHelper.ParseEnum<PriceType>(r[6]),
            WeeklyListFee = CsvHelper.ParseDecimalOrNull(r[7]),
            WeeklyPromoFee = CsvHelper.ParseDecimalOrNull(r[8]),
            TotalListFee = CsvHelper.ParseDecimalOrNull(r[9]),
            TotalPromoFee = CsvHelper.ParseDecimalOrNull(r[10]),
            PromoValidFrom = CsvHelper.ParseDateOrNull(r[11]),
            PromoValidUntil = CsvHelper.ParseDateOrNull(r[12]),
            PackageType = CsvHelper.ParseEnum<PackageType>(r[13]),
            IsAdditional = CsvHelper.ParseBool(r[14]),
            Category = CsvHelper.NullIfEmpty(r[15]),
            MembershipFee = CsvHelper.ParseDecimalOrNull(r[16]),
            RegistrationFee = CsvHelper.ParseDecimalOrNull(r[17]),
            PromotedFee = CsvHelper.ParseDecimalOrNull(r[18]),
            IsOrHasMandatory = CsvHelper.ParseBool(r[19]),
            IsActive = CsvHelper.ParseBool(r[20])
        }).ToList();

    private static List<MainAddOnLink> ReadMainAddOnLinks(ZipArchive zip) =>
        ReadEntry(zip, "MainAddOnLinks.csv").Select(r => new MainAddOnLink
        {
            Id = CsvHelper.ParseInt(r[0]),
            MainPlanId = CsvHelper.ParseInt(r[1]),
            AddOnPlanId = CsvHelper.ParseInt(r[2])
        }).ToList();

    private static List<AccommodationDetail> ReadAccommodationDetails(ZipArchive zip) =>
        ReadEntry(zip, "AccommodationDetails.csv").Select(r => new AccommodationDetail
        {
            PaymentPlanId = CsvHelper.ParseInt(r[0]),
            Type = r[1],
            RoomType = r[2],
            Board = r[3],
            ResidenceName = CsvHelper.NullIfEmpty(r[4]),
            School = r[5],
            Country = r[6],
            Campus = r[7],
            Required = r[8],
            DisplayOption = r[9]
        }).ToList();

    private static List<SupplementDetail> ReadSupplementDetails(ZipArchive zip) =>
        ReadEntry(zip, "SupplementDetails.csv").Select(r => new SupplementDetail
        {
            PaymentPlanId = CsvHelper.ParseInt(r[0]),
            School = r[1],
            Country = r[2],
            Campus = r[3],
            AppliesTo = r[4],
            SupplementName = r[5],
            Required = r[6],
            Notes = CsvHelper.NullIfEmpty(r[7]),
            StartDate = CsvHelper.ParseDateOrNull(r[8]),
            EndDate = CsvHelper.ParseDateOrNull(r[9])
        }).ToList();

    private static List<ExtraServiceDetail> ReadExtraServiceDetails(ZipArchive zip) =>
        ReadEntry(zip, "ExtraServiceDetails.csv").Select(r => new ExtraServiceDetail
        {
            PaymentPlanId = CsvHelper.ParseInt(r[0]),
            Country = r[1],
            VisaType = CsvHelper.NullIfEmpty(r[2]),
            DefaultPriceText = r[3],
            Currency = r[4]
        }).ToList();
}
