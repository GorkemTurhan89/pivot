namespace Pivot.Models.Entities;

public class PaymentPlan
{
    public int Id { get; set; }

    // ExtraServices (Vize / Uçak Bileti) gibi okula bağlı olmayan AddOn'lar için null olabilir.
    public int? SchoolId { get; set; }
    public School? School { get; set; }

    public int? ProgramId { get; set; }
    public CourseProgram? Program { get; set; }

    public string Name { get; set; } = string.Empty;

    // BÜYÜK GEÇİŞ (2026-05-30): tek-değer LengthWeeks ve ValidFrom/ValidTo kaldırıldı.
    // Yerine Excel kaynaklı bant modeli: MinWeek..MaxWeek aralığı + Weekly veya FixedTotal fiyat.
    public int MinWeek { get; set; }
    public int MaxWeek { get; set; }

    public PriceType PriceType { get; set; } = PriceType.Weekly;

    // Weekly tipinde dolu: haftalık liste/promo ücreti (tutar para birimi Country.Currency).
    public decimal? WeeklyListFee { get; set; }
    public decimal? WeeklyPromoFee { get; set; }

    // FixedTotal tipinde dolu: sabit toplam liste/promo ücreti.
    public decimal? TotalListFee { get; set; }
    public decimal? TotalPromoFee { get; set; }

    // Promosyonun geçerlilik aralığı (Excel: PromoValidFrom/Until). Course'un kendi sezon
    // geçerliliği Excel'de yok, o yüzden ValidFrom/ValidTo kaldırıldı.
    public DateOnly? PromoValidFrom { get; set; }
    public DateOnly? PromoValidUntil { get; set; }

    public PackageType PackageType { get; set; } = PackageType.Main;

    public bool IsAdditional { get; set; }

    // Ek paketler için: "Konaklama", "Transfer" vb. Ana paketlerde null.
    public string? Category { get; set; }

    public decimal? MembershipFee { get; set; }
    public decimal? RegistrationFee { get; set; }
    public decimal? PromotedFee { get; set; }

    // Çift amaçlı: AddOn'da true => bu ek paket zorunlu.
    // Main'de true => zorunlu ek hizmeti var (gate); false ise zorunlu AddOn'lara hiç bakılmaz.
    public bool IsOrHasMandatory { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PaymentPlanItem> Items { get; set; } = new List<PaymentPlanItem>();

    // Bu paket Main ise: ona bağlı ek paket linkleri.
    public ICollection<MainAddOnLink> AddOnLinks { get; set; } = new List<MainAddOnLink>();

    // Bu paket AddOn ise: bağlı olduğu ana paket linkleri.
    public ICollection<MainAddOnLink> MainLinks { get; set; } = new List<MainAddOnLink>();
}
