using Microsoft.EntityFrameworkCore;
using Pivot.Models.Entities;

namespace Pivot.Data;

public static class SeedData
{
    public static async Task InitializeAsync(PivotDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Countries.AnyAsync())
            return;

        var uk = new Country { Name = "United Kingdom" };
        var ireland = new Country { Name = "Ireland" };
        var usa = new Country { Name = "United States" };

        var london = new City { Name = "London", Country = uk };
        var brighton = new City { Name = "Brighton", Country = uk };
        var oxford = new City { Name = "Oxford", Country = uk };
        var cambridge = new City { Name = "Cambridge", Country = uk };
        var dublin = new City { Name = "Dublin", Country = ireland };
        var newYork = new City { Name = "New York", Country = usa };

        var schools = new[]
        {
            new School { Name = "Kaplan London Leicester Square", City = london },
            new School { Name = "EC London 30+", City = london },
            new School { Name = "Embassy English Brighton", City = brighton },
            new School { Name = "Kaplan Oxford", City = oxford },
            new School { Name = "Bell Cambridge", City = cambridge },
            new School { Name = "EC Dublin", City = dublin },
            new School { Name = "Kaplan New York Empire State", City = newYork },
        };

        db.Countries.AddRange(uk, ireland, usa);
        db.Schools.AddRange(schools);

        var kaplanOxford = schools.Single(s => s.Name == "Kaplan Oxford");
        var bellCambridge = schools.Single(s => s.Name == "Bell Cambridge");
        var kaplanLondon = schools.Single(s => s.Name == "Kaplan London Leicester Square");

        // Programlar (her okul için)
        var oxfordGE20      = new CourseProgram { Name = "General English 20",   School = kaplanOxford };
        var oxfordIE30      = new CourseProgram { Name = "Intensive English 30", School = kaplanOxford };
        var oxfordAcademic  = new CourseProgram { Name = "Academic Year",         School = kaplanOxford };

        var cambridgeGE20   = new CourseProgram { Name = "General English 20",   School = bellCambridge };
        var cambridgeIE30   = new CourseProgram { Name = "Intensive English 30", School = bellCambridge };

        var londonBusiness  = new CourseProgram { Name = "Business English",     School = kaplanLondon };
        var londonGEMorning = new CourseProgram { Name = "General English Morning", School = kaplanLondon };

        db.Programs.AddRange(oxfordGE20, oxfordIE30, oxfordAcademic,
                             cambridgeGE20, cambridgeIE30,
                             londonBusiness, londonGEMorning);

        var mainPlans = new List<PaymentPlan>
        {
            // Kaplan Oxford - General English 20 (yaz sezonu)
            BuildPlan(oxfordGE20,   "Yaz Sezonu 4w",  4,  "2026-06-01", "2026-09-30", 1200m),
            BuildPlan(oxfordGE20,   "Yaz Sezonu 8w",  8,  "2026-06-01", "2026-09-30", 2200m),
            BuildPlan(oxfordGE20,   "Yaz Sezonu 12w", 12, "2026-06-01", "2026-09-30", 3100m),

            // Kaplan Oxford - Intensive English 30 (yaz sezonu, daha pahalı)
            BuildPlan(oxfordIE30,   "Yaz Sezonu 4w",  4,  "2026-06-01", "2026-09-30", 1600m),
            BuildPlan(oxfordIE30,   "Yaz Sezonu 8w",  8,  "2026-06-01", "2026-09-30", 2950m),

            // Kaplan Oxford - Academic Year (kış sezonu uzun programlar)
            BuildPlan(oxfordAcademic, "Kış Sezonu 8w",  8,  "2026-01-10", "2026-05-31", 2000m),
            BuildPlan(oxfordAcademic, "Kış Sezonu 12w", 12, "2026-01-10", "2026-05-31", 2900m),
            BuildPlan(oxfordAcademic, "Kış Sezonu 24w", 24, "2026-01-10", "2026-05-31", 5400m),

            // Bell Cambridge - General English 20
            BuildPlan(cambridgeGE20, "Standart 4w",  4,  "2026-05-01", "2026-10-31", 1400m),
            BuildPlan(cambridgeGE20, "Standart 8w",  8,  "2026-05-01", "2026-10-31", 2600m),

            // Bell Cambridge - Intensive English 30
            BuildPlan(cambridgeIE30, "Standart 4w",  4,  "2026-05-01", "2026-10-31", 1800m),

            // Kaplan London - Business
            BuildPlan(londonBusiness, "Yıl Boyu 2w", 2, "2026-01-01", "2026-12-31", 800m),
            BuildPlan(londonBusiness, "Yıl Boyu 4w", 4, "2026-01-01", "2026-12-31", 1500m),

            // Kaplan London - GE Morning
            BuildPlan(londonGEMorning, "Yıl Boyu 4w", 4, "2026-01-01", "2026-12-31", 1100m),
        };

        // Opsiyonel ek paketler (zorunlu değil) - okul bazlı, program bağımsız
        var optionalAddOns = new List<PaymentPlan>
        {
            BuildAddOn(kaplanOxford,  "Standart Yurt - Tek Kişilik Oda (haftalık)", 280m, "Konaklama"),
            BuildAddOn(kaplanOxford,  "Havalimanı Transferi (Heathrow)", 150m, "Transfer"),
            BuildAddOn(bellCambridge, "Aile Yanı Konaklama (haftalık)", 220m, "Konaklama"),
            BuildAddOn(kaplanLondon,  "Havalimanı Transferi (Heathrow)", 130m, "Transfer"),
        };

        // Zorunlu ek paketler + ana paketlere bağlantılar
        var mandatoryAddOns = new List<PaymentPlan>();
        var links = new List<MainAddOnLink>();

        // Materyal: her okul için tek bir zorunlu AddOn, o okulun TÜM ana paketlerine bağlı (1 addon -> N main)
        foreach (var school in new[] { kaplanOxford, bellCambridge, kaplanLondon })
        {
            var material = BuildAddOn(school, "Zorunlu Materyal Ücreti", 60m, "Materyal", isMandatory: true);
            mandatoryAddOns.Add(material);

            foreach (var main in mainPlans.Where(p => p.School == school))
            {
                links.Add(new MainAddOnLink { MainPlan = main, AddOnPlan = material });
                main.IsOrHasMandatory = true;
            }
        }

        // Sağlık sigortası: aynı okulda uzunluğa göre AYRI kayıt (per-variant: 2w vs 4w farklı fiyat)
        var londonBiz2w = mainPlans.Single(p => p.Program == londonBusiness && p.LengthWeeks == 2);
        var londonBiz4w = mainPlans.Single(p => p.Program == londonBusiness && p.LengthWeeks == 4);

        var insurance2w = BuildAddOn(kaplanLondon, "Zorunlu Sağlık Sigortası (2 hafta)", 40m, "Sigorta", isMandatory: true);
        var insurance4w = BuildAddOn(kaplanLondon, "Zorunlu Sağlık Sigortası (4 hafta)", 75m, "Sigorta", isMandatory: true);
        mandatoryAddOns.Add(insurance2w);
        mandatoryAddOns.Add(insurance4w);

        links.Add(new MainAddOnLink { MainPlan = londonBiz2w, AddOnPlan = insurance2w });
        links.Add(new MainAddOnLink { MainPlan = londonBiz4w, AddOnPlan = insurance4w });

        db.PaymentPlans.AddRange(mainPlans);
        db.PaymentPlans.AddRange(optionalAddOns);
        db.PaymentPlans.AddRange(mandatoryAddOns);
        db.MainAddOnLinks.AddRange(links);

        await db.SaveChangesAsync();
    }

    private static PaymentPlan BuildPlan(CourseProgram program, string name, int weeks, string from, string to, decimal totalPrice)
    {
        var plan = new PaymentPlan
        {
            School = program.School,
            Program = program,
            Name = name,
            LengthWeeks = weeks,
            ValidFrom = DateOnly.Parse(from),
            ValidTo = DateOnly.Parse(to),
            PackageType = PackageType.Main,
            IsAdditional = false,
            RegistrationFee = 95m,
            IsActive = true
        };

        // Kayıt ücreti RegistrationFee alanında, materyal zorunlu AddOn olarak link'leniyor (ikisi de item değil).
        // Ders ücreti = toplam - kayıt(95) - materyal(60).
        plan.Items.Add(new PaymentPlanItem { ItemName = $"Ders ücreti ({weeks} hafta)", ItemPrice = totalPrice - 155m, Currency = "GBP", Quantity = 1 });
        return plan;
    }

    private static PaymentPlan BuildAddOn(School school, string name, decimal price, string category, bool isMandatory = false)
    {
        var plan = new PaymentPlan
        {
            School = school,
            Program = null,
            Name = name,
            LengthWeeks = 0,
            ValidFrom = DateOnly.Parse("2026-01-01"),
            ValidTo = DateOnly.Parse("2026-12-31"),
            PackageType = PackageType.AddOn,
            IsAdditional = true,
            Category = category,
            IsOrHasMandatory = isMandatory,
            IsActive = true
        };
        plan.Items.Add(new PaymentPlanItem { ItemName = name, ItemPrice = price, Currency = "GBP", Quantity = 1 });
        return plan;
    }
}
